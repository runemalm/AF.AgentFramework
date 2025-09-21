using AgentFramework.Kernel;
using AgentFramework.Runners;

namespace AgentFramework.Engines.Reactive;

/// <summary>
/// Skeleton ReactiveEngine: binds to Kernel and manages runners (e.g., webhooks/queues).
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

    public void BindKernel(IKernel kernel) => _kernel = kernel ?? throw new ArgumentNullException(nameof(kernel));

    public void AddRunner(IRunner runner)
    {
        if (runner is null) throw new ArgumentNullException(nameof(runner));
        _runners.Add(runner);
        // In a real impl, runners would call back with events → enqueue Percept items for _agentIds.
    }

    public void SetAttachments(IReadOnlyList<string> agentIds)
    {
        _agentIds.Clear();
        if (agentIds is not null) _agentIds.AddRange(agentIds);
        Console.WriteLine($"[Engine] {Id} attachments set: {string.Join(", ", _agentIds)}");
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