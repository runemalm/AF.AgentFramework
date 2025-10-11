using AgentFramework.Kernel;
using AgentFramework.Runners;

namespace AgentFramework.Engines.Reactive;

/// <summary>
/// Reactive engine: receives external events from its runners and
/// converts them into percept work items for all attached agents.
/// </summary>
public sealed class ReactiveEngine : IEngine
{
    private readonly List<IRunner> _runners = new();
    private readonly List<string> _agentIds = new();
    private IKernel? _kernel;

    public string Id { get; }

    public ReactiveEngine(string id = "reactive")
    {
        Id = id;
    }

    public void BindKernel(IKernel kernel) =>
        _kernel = kernel ?? throw new ArgumentNullException(nameof(kernel));

    public void AddRunner(IRunner runner)
    {
        if (runner is null) throw new ArgumentNullException(nameof(runner));
        _runners.Add(runner);

        // Wire the runner to the engine if it supports reactive events
        if (runner is IReactiveRunner reactive)
            reactive.OnEventAsync = OnPerceptAsync;
    }

    public void SetAttachments(IReadOnlyList<string> agentIds)
    {
        _agentIds.Clear();
        if (agentIds is not null) _agentIds.AddRange(agentIds);
        Console.WriteLine($"[Engine] {Id} attachments set: {string.Join(", ", _agentIds)}");
    }

    /// <summary>
    /// Called by runners when an external event occurs.
    /// Converts the event into percept work items for attached agents.
    /// </summary>
    public async Task OnPerceptAsync(object? payload, string? source, CancellationToken ct = default)
    {
        if (_kernel is null) return;

        foreach (var agentId in _agentIds)
        {
            var item = new WorkItem
            {
                Id = Guid.NewGuid().ToString("n"),
                EngineId = Id,
                AgentId = agentId,
                Kind = WorkItemKind.Percept,
                Payload = payload,
                Metadata = new Dictionary<string, string>
                {
                    ["source"] = source ?? "unknown"
                }
            };

            Console.WriteLine($"[Engine] {Id} enqueue Percept for '{agentId}' (src={source ?? "?"}).");
            await _kernel.EnqueueAsync(item, ct).ConfigureAwait(false);
        }
    }

    public async Task StartAsync(CancellationToken ct = default)
    {
        Console.WriteLine($"[Engine] ReactiveEngine '{Id}' starting with {_runners.Count} runner(s).");
        foreach (var r in _runners)
            await r.StartAsync(ct).ConfigureAwait(false);
        Console.WriteLine($"[Engine] ReactiveEngine '{Id}' started.");
    }

    public async Task StopAsync(CancellationToken ct = default)
    {
        Console.WriteLine($"[Engine] ReactiveEngine '{Id}' stopping...");
        for (var i = _runners.Count - 1; i >= 0; i--)
            await _runners[i].StopAsync(ct).ConfigureAwait(false);
        Console.WriteLine($"[Engine] ReactiveEngine '{Id}' stopped.");
    }
}
