using System.Collections.Concurrent;
using AgentFramework.Kernel.Policies;
using AgentFramework.Kernel.Policies.Defaults;

namespace AgentFramework.Kernel;

/// <summary>
/// In-process kernel with per-agent mailboxes and a single coordinator loop (v1).
/// Guarantees SingleActive per agent. Honors basic Admission/Ordering/Timeout/Retry/Preemption/Backpressure.
/// </summary>
public sealed class InProcKernel : IKernel, IDisposable
{
    private readonly IAgentCatalog _catalog;
    private readonly PolicySet _defaults;
    private readonly Dictionary<(string AgentId, string EngineId), PolicySet> _bindingPolicies;
    private readonly Dictionary<string, AgentEntry> _agents = new(StringComparer.Ordinal);

    private readonly CancellationTokenSource _cts = new();
    private Task? _coordinator;

    // attempt tracking per WorkItem.Id
    private readonly ConcurrentDictionary<string, int> _attempts = new();

    // simple global counts
    private int _running;
    
    private sealed class AttachmentKeyComparer : IEqualityComparer<(string AgentId, string EngineId)>
    {
        public bool Equals((string AgentId, string EngineId) x, (string AgentId, string EngineId) y) =>
            string.Equals(x.AgentId, y.AgentId, StringComparison.Ordinal)
            && string.Equals(x.EngineId, y.EngineId, StringComparison.Ordinal);

        public int GetHashCode((string AgentId, string EngineId) obj) =>
            HashCode.Combine(obj.AgentId, obj.EngineId);
    }

    public InProcKernel(KernelOptions options)
    {
        _catalog = options.Agents ?? throw new ArgumentNullException(nameof(options.Agents));
        _defaults = options.Defaults ?? PolicySetDefaults.Create();
        _bindingPolicies = (options.Bindings ?? Array.Empty<AttachmentBinding>())
            .ToDictionary(b => (b.AgentId, b.EngineId), b => b.Policies, new AttachmentKeyComparer());
    }

    public Task StartAsync(CancellationToken ct = default)
    {
        Console.WriteLine("[Kernel] Starting…");
        _coordinator = Task.Run(CoordinatorLoop, _cts.Token);
        Console.WriteLine("[Kernel] Started.");
        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken ct = default)
    {
        Console.WriteLine("[Kernel] Stopping…");
        _cts.Cancel();
        if (_coordinator is not null)
        {
            try { await _coordinator.ConfigureAwait(false); } catch (OperationCanceledException) { }
        }
        Console.WriteLine("[Kernel] Stopped.");
    }

    public ValueTask EnqueueAsync(WorkItem item, CancellationToken ct = default)
    {
        if (item is null) throw new ArgumentNullException(nameof(item));
        var now = DateTimeOffset.UtcNow;

        var pol = ResolvePolicies(item.AgentId, item.EngineId);

        // Backpressure (cluster-level)
        var cluster = new ClusterLoad(TotalQueued: _agents.Values.Sum(a => a.QueueCount), TotalRunning: _running);
        var bp = (pol.Backpressure ?? _defaults.Backpressure ?? new DefaultBackpressurePolicy()).Evaluate(cluster);
        if (bp == BackpressureDecision.Shed)
        {
            Console.WriteLine($"[Kernel] Shed: {Describe(item)}");
            return ValueTask.CompletedTask;
        }

        // Admission (per-agent)
        var entry = GetAgentEntry(item.AgentId, pol);
        var state = new AgentRuntimeState(entry.IsRunning, entry.QueueCount);
        var admit = (pol.Admission ?? _defaults.Admission ?? new DefaultAdmissionPolicy()).Admit(item, state);

        if (admit == AdmissionDecision.Reject)
        {
            Console.WriteLine($"[Kernel] Rejected: {Describe(item)}");
            return ValueTask.CompletedTask;
        }

        // Defer => treat as accept in v1 (TODO: small delay/retry)
        entry.Enqueue(new QueuedItem(item, pol, notBefore: now));

        // Preemption (cooperative)
        if (entry.IsRunning && entry.Running is { } running)
        {
            var decision = (pol.Preemption ?? _defaults.Preemption ?? new DefaultPreemptionPolicy())
                .ShouldPreempt(item, running);
            if (decision == PreemptionDecision.Cooperative)
            {
                Console.WriteLine($"[Kernel] Preempt requested by {Describe(item)}; canceling current.");
                entry.CancelRunning();
                // boost: move the item to head by marking as urgent
                entry.Boost(item.Id);
            }
        }

        return ValueTask.CompletedTask;
    }

    private async Task CoordinatorLoop()
    {
        var token = _cts.Token;
        var idleDelay = TimeSpan.FromMilliseconds(25);

        while (!token.IsCancellationRequested)
        {
            bool didWork = false;
            foreach (var entry in SnapshotAgents())
            {
                if (token.IsCancellationRequested) break;

                if (entry.TryDequeueNext(out var qitem))
                {
                    didWork = true;
                    Interlocked.Increment(ref _running);

                    var timeout = (qitem.Policies.Timeout ?? _defaults.Timeout)?.GetTimeout(qitem.WorkItem);
                    using var linkedCts = timeout is { } ts && ts > TimeSpan.Zero
                        ? CancellationTokenSource.CreateLinkedTokenSource(token)
                        : CancellationTokenSource.CreateLinkedTokenSource(token);

                    if (timeout is { } t && t > TimeSpan.Zero)
                        linkedCts.CancelAfter(t);

                    var attempt = _attempts.AddOrUpdate(qitem.WorkItem.Id, 1, (_, prev) => prev + 1);
                    var ctx = new AgentContext(qitem.WorkItem.AgentId, qitem.WorkItem.EngineId, qitem.WorkItem.Id,
                                               qitem.WorkItem.CorrelationId, linkedCts.Token, randomSeed: qitem.WorkItem.Id.GetHashCode());

                    entry.MarkRunning(qitem, linkedCts);

                    try
                    {
                        var agent = _catalog.Get(qitem.WorkItem.AgentId);
                        Console.WriteLine($"[Kernel] Dispatch → Agent={agent.Id} Item={qitem.WorkItem.Id} Kind={qitem.WorkItem.Kind} (Attempt {attempt})");
                        await agent.HandleAsync(qitem.WorkItem, ctx).ConfigureAwait(false);
                        Console.WriteLine($"[Kernel] Complete ← Agent={agent.Id} Item={qitem.WorkItem.Id}");
                    }
                    catch (OperationCanceledException)
                    {
                        Console.WriteLine($"[Kernel] Canceled ← Agent={qitem.WorkItem.AgentId} Item={qitem.WorkItem.Id}");
                        // no retry on cancellations
                    }
                    catch (Exception ex)
                    {
                        var retry = (qitem.Policies.Retry ?? _defaults.Retry ?? new DefaultRetryPolicy())
                            .OnFailure(qitem.WorkItem, ex, attempt);
                        if (retry.ShouldRetry)
                        {
                            var notBefore = DateTimeOffset.UtcNow + (retry.Delay ?? TimeSpan.Zero);
                            Console.WriteLine($"[Kernel] Failed → retry in {retry.Delay?.TotalMilliseconds ?? 0} ms: {Describe(qitem.WorkItem)}");
                            entry.Enqueue(new QueuedItem(qitem.WorkItem, qitem.Policies, notBefore));
                        }
                        else
                        {
                            Console.WriteLine($"[Kernel] Failed → no retry: {Describe(qitem.WorkItem)} ({retry.Reason})");
                        }
                    }
                    finally
                    {
                        entry.MarkIdle();
                        Interlocked.Decrement(ref _running);
                    }
                }
            }

            if (!didWork)
                await Task.Delay(idleDelay, token).ConfigureAwait(false);
        }
    }

    private IEnumerable<AgentEntry> SnapshotAgents()
    {
        // Copy to avoid dictionary enumeration exceptions while enqueueing
        lock (_agents) return _agents.Values.ToList();
    }

    private PolicySet ResolvePolicies(string agentId, string engineId)
        => _bindingPolicies.TryGetValue((agentId, engineId), out var p) ? p : _defaults;

    private AgentEntry GetAgentEntry(string agentId, PolicySet activePolicies)
    {
        lock (_agents)
        {
            if (_agents.TryGetValue(agentId, out var existing)) return existing;
            var ordering = activePolicies.Ordering ?? _defaults.Ordering ?? new DefaultOrderingPolicy();
            var entry = new AgentEntry(agentId, ordering);
            _agents.Add(agentId, entry);
            return entry;
        }
    }

    private static string Describe(WorkItem i) => $"[{i.Kind}] a={i.AgentId} e={i.EngineId} id={i.Id}";

    public void Dispose() => _cts.Cancel();

    // --- Internal helper types ---

    private sealed class QueuedItem
    {
        public WorkItem WorkItem { get; }
        public PolicySet Policies { get; }
        public DateTimeOffset NotBefore { get; set; }
        public bool Boosted { get; set; }

        public QueuedItem(WorkItem wi, PolicySet p, DateTimeOffset notBefore)
        {
            WorkItem = wi;
            Policies = p;
            NotBefore = notBefore;
        }
    }

    private sealed class AgentEntry
    {
        private readonly List<QueuedItem> _queue = new();
        private readonly object _sync = new();
        private readonly IOrderingPolicy _ordering;

        public string AgentId { get; }
        public bool IsRunning { get; private set; }
        public RunningInvocation? Running { get; private set; }

        public AgentEntry(string agentId, IOrderingPolicy ordering)
        {
            AgentId = agentId;
            _ordering = ordering;
        }

        public int QueueCount { get { lock (_sync) return _queue.Count; } }

        public void Enqueue(QueuedItem item)
        {
            lock (_sync)
            {
                _queue.Add(item);
            }
        }

        public void Boost(string workItemId)
        {
            lock (_sync)
            {
                var qi = _queue.FirstOrDefault(q => q.WorkItem.Id == workItemId);
                if (qi is not null) qi.Boosted = true;
            }
        }

        public bool TryDequeueNext(out QueuedItem item)
        {
            lock (_sync)
            {
                item = null!;
                if (IsRunning || _queue.Count == 0) return false;

                var now = DateTimeOffset.UtcNow;

                // Pick best candidate: boosted first; otherwise by ordering; skip NotBefore in future
                var candidates = _queue.Where(q => q.NotBefore <= now).ToList();
                if (candidates.Count == 0) return false;

                // Prefer Boosted items
                var boosted = candidates.FirstOrDefault(q => q.Boosted);
                if (boosted is not null)
                {
                    _queue.Remove(boosted);
                    item = boosted;
                    return true;
                }

                // Order by policy (fallback to priority/deadline via default policy)
                candidates.Sort((x, y) => _ordering.Compare(x.WorkItem, y.WorkItem));
                var chosen = candidates.First();
                _queue.Remove(chosen);
                item = chosen;
                return true;
            }
        }

        public void MarkRunning(QueuedItem qi, CancellationTokenSource linkedCts)
        {
            lock (_sync)
            {
                IsRunning = true;
                Running = new RunningInvocation(qi.WorkItem, DateTimeOffset.UtcNow);
                _runningCts = linkedCts;
            }
        }

        public void MarkIdle()
        {
            lock (_sync)
            {
                IsRunning = false;
                Running = null;
                _runningCts?.Dispose();
                _runningCts = null;
            }
        }
        
        public void CancelRunning()
        {
            lock (_sync)
            {
                _runningCts?.Cancel();
            }
        }

        private CancellationTokenSource? _runningCts;
    }
}
