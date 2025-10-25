using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using AgentFramework.Kernel.Diagnostics;
using AgentFramework.Kernel.Policies;
using AgentFramework.Kernel.Policies.Defaults;
using AgentFramework.Tools.Integration;
using Microsoft.Extensions.Logging.Abstractions;

namespace AgentFramework.Kernel;

/// <summary>
/// In-process kernel with per-agent mailboxes and a single coordinator loop (v1).
/// Guarantees SingleActive per agent. Honors basic Admission/Ordering/Timeout/Retry/Preemption/Backpressure.
/// </summary>
public sealed class InProcKernel : IKernel, IKernelInspector, IDisposable
{
    private readonly ILogger<InProcKernel> _logger;
    private readonly IAgentCatalog _catalog;
    private readonly PolicySet _defaults;
    private readonly Dictionary<(string AgentId, string EngineId), PolicySet> _bindingPolicies;
    private readonly int _workerCount;
    private readonly ToolSubsystemFactory? _toolFactory;
    private readonly List<IAgentMetricsProvider> _metricsProviders = new();
    private readonly Dictionary<string, AgentEntry> _agents = new(StringComparer.Ordinal);
    private double _throughput;
    private int _lastHandledCount;
    private DateTimeOffset _lastThroughputSample = DateTimeOffset.UtcNow;
    private readonly CancellationTokenSource _cts = new();
    private int _running;
    private Task? _throughputSampler;
    private readonly TimeSpan _throughputSamplePeriod = TimeSpan.FromMilliseconds(500);
    private readonly List<Task> _workers = new();
    private readonly object _scheduleLock = new();
    private readonly ConcurrentDictionary<string, int> _attempts = new();
    
    private sealed class AttachmentKeyComparer : IEqualityComparer<(string AgentId, string EngineId)>
    {
        public bool Equals((string AgentId, string EngineId) x, (string AgentId, string EngineId) y) =>
            string.Equals(x.AgentId, y.AgentId, StringComparison.Ordinal)
            && string.Equals(x.EngineId, y.EngineId, StringComparison.Ordinal);

        public int GetHashCode((string AgentId, string EngineId) obj) =>
            HashCode.Combine(obj.AgentId, obj.EngineId);
    }

    public InProcKernel(KernelOptions options, ILogger<InProcKernel>? logger = null)
    {
        _catalog = options.Agents ?? throw new ArgumentNullException(nameof(options.Agents));
        _logger = logger ?? NullLogger<InProcKernel>.Instance;
        _defaults = options.Defaults ?? PolicySetDefaults.Create();
        _bindingPolicies = (options.Bindings ?? Array.Empty<AttachmentBinding>())
            .ToDictionary(b => (b.AgentId, b.EngineId), b => b.Policies, new AttachmentKeyComparer());
        _workerCount = options.WorkerCount;
        _toolFactory = options.ToolFactory;
        
        if (options.MetricsProviders is not null)
            _metricsProviders.AddRange(options.MetricsProviders);
    }
    
    public Task StartAsync(CancellationToken ct = default)
    {
        Console.WriteLine($"[Kernel] Starting with {_workerCount} workers on {Environment.ProcessorCount} logical processors…");

        for (int i = 0; i < _workerCount; i++)
        {
            int id = i; // capture loop variable
            _workers.Add(Task.Run(() => WorkerLoop(id, _cts.Token)));
        }
        
        // Start periodic throughput sampler
        _throughputSampler = Task.Run(() => ThroughputSamplerLoop(_cts.Token));

        Console.WriteLine("[Kernel] Started.");
        return Task.CompletedTask;
    }
    
    public async Task StopAsync(CancellationToken ct = default)
    {
        Console.WriteLine("[Kernel] Stopping…");
        _cts.Cancel();

        var allTasks = new List<Task>(_workers);
        if (_throughputSampler is not null)
            allTasks.Add(_throughputSampler);

        try
        {
            await Task.WhenAll(allTasks).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            // expected
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

        switch (admit)
        {
            case AdmissionDecision.Reject:
                entry.IncrementRejected();
                Console.WriteLine($"[Kernel] Rejected: {Describe(item)}");
                return ValueTask.CompletedTask;

            case AdmissionDecision.Defer:
                // Defer => in v1, treat as delayed enqueue (could add jitter later)
                entry.Enqueue(new QueuedItem(item, pol, notBefore: now + TimeSpan.FromMilliseconds(50)));
                return ValueTask.CompletedTask;

            default: // Accept
                entry.Enqueue(new QueuedItem(item, pol, notBefore: now));
                break;
        }

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
    
    private async Task WorkerLoop(int workerId, CancellationToken ct)
    {
        var idleDelay = TimeSpan.FromMilliseconds(25);

        while (!ct.IsCancellationRequested)
        {
            bool didWork = false;
            AgentEntry? entry = null;

            // --- 1. Select next agent (short critical section) ---
            lock (_scheduleLock)
            {
                if (_agents.Count > 0)
                {
                    // Build a lightweight agent view list for the scheduler
                    var views = _agents.Values.Select(a => a.AsView()).ToList();

                    // Compute scheduling context (cluster-level info)
                    var context = new SchedulingContext(
                        TotalRunning: _running,
                        TotalQueued: _agents.Sum(a => a.Value.QueueCount),
                        Now: DateTimeOffset.UtcNow
                    );

                    // Pick scheduling policy (default if none bound)
                    var policy = _defaults.Scheduling ?? new DefaultSchedulingPolicy();
                    var selection = policy.SelectNext(views, context);

                    if (selection is not null)
                        _agents.TryGetValue(selection.Value.AgentId, out entry);
                }
            }

            // --- 2. Execute outside lock ---
            if (entry?.TryDequeueNext(out var qitem) == true)
            {
                didWork = true;

                try
                {
                    await ExecuteAgentAsync(entry, qitem, ct).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    // expected when shutting down
                }
            }

            // --- 3. Idle backoff ---
            if (!didWork)
                await Task.Delay(idleDelay, ct).ConfigureAwait(false);
        }

        Console.WriteLine($"[Worker {workerId}] exiting.");
    }
    
    private async Task ThroughputSamplerLoop(CancellationToken ct)
    {
        await Task.Delay(_throughputSamplePeriod, ct); // warm-up
        while (!ct.IsCancellationRequested)
        {
            try
            {
                UpdateThroughput();
                await Task.Delay(_throughputSamplePeriod, ct).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    private void UpdateThroughput()
    {
        lock (_agents)
        {
            int totalHandled = _agents.Values.Sum(a => a.TotalHandled);
            var now = DateTimeOffset.UtcNow;
            var dt = (now - _lastThroughputSample).TotalSeconds;
            var diff = totalHandled - _lastHandledCount;
            var current = dt > 0 ? diff / dt : 0;

            // Same exponential smoothing, but now decoupled from dashboard call rate
            _throughput = _throughput * 0.8 + current * 0.2;

            _lastHandledCount = totalHandled;
            _lastThroughputSample = now;
        }
    }
    
    private async Task ExecuteAgentAsync(AgentEntry entry, QueuedItem qitem, CancellationToken kernelToken)
    {
        Interlocked.Increment(ref _running);

        var timeout = (qitem.Policies.Timeout ?? _defaults.Timeout)?.GetTimeout(qitem.WorkItem);
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(kernelToken);

        if (timeout is { } ts && ts > TimeSpan.Zero)
            linkedCts.CancelAfter(ts);

        var attempt = _attempts.AddOrUpdate(qitem.WorkItem.Id, 1, (_, prev) => prev + 1);

        var tools = _toolFactory?.Invoke(qitem.WorkItem.AgentId);

        var ctx = new AgentContext(
            qitem.WorkItem.AgentId,
            qitem.WorkItem.EngineId,
            qitem.WorkItem.Id,
            qitem.WorkItem.CorrelationId,
            linkedCts.Token,
            randomSeed: qitem.WorkItem.Id.GetHashCode(),
            knowledge: null,
            tools: tools
        );

        entry.AttachRunContext(linkedCts);

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
    
    public KernelSnapshot GetSnapshot()
    {
        List<AgentSnapshot> agents;
        int totalRejected, totalHandled;

        lock (_agents)
        {
            agents = _agents.Values
                .Select(entry =>
                {
                    var snapshot = entry.ToSnapshot(entry.QueueCount);

                    // Merge metrics from all registered providers
                    if (_metricsProviders.Count > 0)
                    {
                        var merged = new Dictionary<string, object>(StringComparer.Ordinal);
                        foreach (var provider in _metricsProviders)
                        {
                            try
                            {
                                var metrics = provider.TryGetAgentMetrics(entry.AgentId);
                                if (metrics is null) continue;

                                foreach (var kv in metrics.Metrics)
                                    merged[kv.Key] = kv.Value;
                            }
                            catch (Exception ex)
                            {
                                // Defensive: provider errors should never break snapshot creation
                                Console.WriteLine($"[Kernel] Metrics provider {provider.GetType().Name} failed: {ex.Message}");
                            }
                        }

                        if (merged.Count > 0)
                            snapshot = snapshot.WithMetrics(merged);
                    }

                    return snapshot;
                })
                .ToList();

            totalRejected = agents.Sum(a => a.Rejected);
            totalHandled  = agents.Sum(a => a.TotalHandled);
        }

        var now = DateTimeOffset.UtcNow;
        var totalAgents = agents.Count;
        var running = agents.Count(a => a.IsRunning);
        var queued = agents.Sum(a => a.QueueLength);

        return new KernelSnapshot(
            TotalAgents: totalAgents,
            RunningAgents: running,
            QueuedItems: queued,
            RejectedItems: totalRejected,
            TotalHandledItems: totalHandled,
            ThroughputPerSecond: Math.Round(_throughput, 2),
            Agents: agents,
            Timestamp: now,
            WorkerCount: _workerCount
        );
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
        private readonly AgentStats _stats = new();

        public string AgentId { get; }
        public bool IsRunning { get; private set; }
        public RunningInvocation? Running { get; private set; }

        private CancellationTokenSource? _runningCts;
        private DateTimeOffset _runStart;
        private int _lastQueueCount;

        public AgentEntry(string agentId, IOrderingPolicy ordering)
        {
            AgentId = agentId;
            _ordering = ordering;
            _stats.Created = DateTimeOffset.UtcNow;
            _stats.LastSample = _stats.Created;
        }
        
        public int TotalHandled
        {
            get { lock (_sync) return _stats.TotalHandled; }
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

                var boosted = candidates.FirstOrDefault(q => q.Boosted);
                if (boosted is not null)
                {
                    _queue.Remove(boosted);
                    item = boosted;
                    return true;
                }

                candidates.Sort((x, y) => _ordering.Compare(x.WorkItem, y.WorkItem));
                var chosen = candidates.First();
                _queue.Remove(chosen);

                // 🔒 Atomically mark running
                IsRunning = true;
                Running = new RunningInvocation(chosen.WorkItem, DateTimeOffset.UtcNow);
                _runStart = DateTimeOffset.UtcNow;

                item = chosen;
                return true;
            }
        }

        public void AttachRunContext(CancellationTokenSource linkedCts)
        {
            lock (_sync)
            {
                _runningCts = linkedCts;
            }
        }

        public void MarkIdle()
        {
            lock (_sync)
            {
                if (IsRunning)
                {
                    var elapsed = DateTimeOffset.UtcNow - _runStart;
                    _stats.ActiveTime += elapsed;
                    _stats.TotalHandled++;
                    _stats.AvgExecutionMs = _stats.AvgExecutionMs * 0.8 + elapsed.TotalMilliseconds * 0.2;
                }

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

        public IAgentView AsView() => new View(this);

        public AgentSnapshot ToSnapshot(int currentQueue)
        {
            var now = DateTimeOffset.UtcNow;
            var dt = (now - _stats.LastSample).TotalSeconds;
            var dq = currentQueue - _lastQueueCount;
            var growth = dt > 0 ? dq / dt : 0;

            _stats.QueueGrowthRate = _stats.QueueGrowthRate * 0.8 + growth * 0.2;
            _stats.LastSample = now;
            _lastQueueCount = currentQueue;

            var uptime = (now - _stats.Created).TotalSeconds;
            var util = uptime > 0 ? (_stats.ActiveTime.TotalSeconds / uptime) * 100 : 0;

            return new AgentSnapshot(
                Id: AgentId,
                QueueLength: currentQueue,
                IsRunning: IsRunning,
                TotalHandled: _stats.TotalHandled,
                Rejected: _stats.Rejected,
                AvgExecutionMs: Math.Round(_stats.AvgExecutionMs, 1),
                QueueGrowthRate: Math.Round(_stats.QueueGrowthRate, 1),
                UtilizationPercent: Math.Round(util, 1)
            );
        }

        public void IncrementRejected()
        {
            lock (_sync) { _stats.Rejected++; }
        }

        // --- Internal lightweight stats struct ---
        private sealed class AgentStats
        {
            public DateTimeOffset Created { get; set; }
            public DateTimeOffset LastSample { get; set; }
            public TimeSpan ActiveTime { get; set; }
            public double AvgExecutionMs { get; set; }
            public double QueueGrowthRate { get; set; }
            public int TotalHandled { get; set; }
            public int Rejected { get; set; }
        }

        private sealed class View : IExtendedAgentView
        {
            private readonly AgentEntry _inner;
            public View(AgentEntry inner) => _inner = inner;
            public string Id => _inner.AgentId;
            public int QueueLength => _inner.QueueCount;
            public bool IsRunning => _inner.IsRunning;
            public double UtilizationPercent => _inner.ToSnapshot(_inner.QueueCount).UtilizationPercent;
            public double AvgExecutionMs => _inner.ToSnapshot(_inner.QueueCount).AvgExecutionMs;
        }
    }
}
