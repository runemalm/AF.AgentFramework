using AgentFramework.Kernel;
using AgentFramework.Kernel.Routing;
using AgentFramework.Runners;

namespace AgentFramework.Engines.Reactive;

/// <summary>
/// Reactive engine: receives external stimuli from its runners and
/// converts them into percept work items for attached agents using a router.
/// </summary>
public sealed class ReactiveEngine : IEngine
{
    private readonly List<IRunner> _runners = new();
    private readonly List<string> _agentIds = new();
    private IKernel? _kernel;
    private readonly IStimulusRouter _router;

    public string Id { get; }

    /// <summary>
    /// Creates a new reactive engine with a specific ID and stimulus router.
    /// The router decides how incoming percepts are distributed to agents.
    /// </summary>
    public ReactiveEngine(string id, IStimulusRouter router)
    {
        Id = id ?? throw new ArgumentNullException(nameof(id));
        _router = router ?? throw new ArgumentNullException(nameof(router));
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
    /// Converts the event into a percept WorkItem and routes it to interested agents.
    /// </summary>
    public async Task OnPerceptAsync(object? payload, string? topic, CancellationToken ct = default)
    {
        if (_kernel is null) return;

        var percept = new WorkItem
        {
            Id = Guid.NewGuid().ToString("n"),
            EngineId = Id,
            AgentId = string.Empty, // router decides recipients
            Kind = WorkItemKind.Percept,
            Payload = payload,
            Metadata = new Dictionary<string, string>
            {
                ["topic"] = topic ?? "unknown"
            }
        };

        Console.WriteLine($"[Engine] {Id} received percept (topic={topic ?? "?"}).");
        await _router.RouteAsync(percept, _kernel, _agentIds, ct).ConfigureAwait(false);
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
