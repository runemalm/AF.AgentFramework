using AgentFramework.Kernel;
using AgentFramework.Runners;
using AgentFramework.Runners.Timers;

namespace AgentFramework.Engines.Loop;

/// <summary>
/// Skeleton LoopEngine: binds to Kernel, manages runners, and enqueues Tick work items for attached agents.
/// </summary>
public sealed class LoopEngine : IEngine
{
    private readonly List<IRunner> _runners = new();
    private readonly List<string> _agentIds = new();
    private IKernel? _kernel;

    public string Id { get; }

    public LoopEngine(string id = "loop")
    {
        Id = id;
    }

    public void BindKernel(IKernel kernel) => _kernel = kernel ?? throw new ArgumentNullException(nameof(kernel));

    public void AddRunner(IRunner runner)
    {
        if (runner is null) throw new ArgumentNullException(nameof(runner));
        _runners.Add(runner);

        // If it's our TimerRunner, hook its tick to enqueue work
        if (runner is TimerRunner t)
        {
            t.OnTickAsync = async ct =>
            {
                if (_kernel is null) return;
                foreach (var agentId in _agentIds)
                {
                    var item = new WorkItem
                    {
                        Id = Guid.NewGuid().ToString("n"),
                        EngineId = Id,
                        AgentId = agentId,
                        Kind = WorkItemKind.Tick,
                        Priority = 0,
                        Payload = null
                    };
                    Console.WriteLine($"[Engine] {Id} enqueue Tick for agent '{agentId}' ({item.Id}).");
                    await _kernel.EnqueueAsync(item, ct).ConfigureAwait(false);
                }
            };
        }
    }

    public void SetAttachments(IReadOnlyList<string> agentIds)
    {
        _agentIds.Clear();
        if (agentIds is not null) _agentIds.AddRange(agentIds);
        Console.WriteLine($"[Engine] {Id} attachments set: {string.Join(", ", _agentIds)}");
    }

    public async Task StartAsync(CancellationToken ct = default)
    {
        Console.WriteLine($"[Engine] LoopEngine '{Id}' starting with {_runners.Count} runner(s).");
        foreach (var r in _runners)
            await r.StartAsync(ct).ConfigureAwait(false);
        Console.WriteLine($"[Engine] LoopEngine '{Id}' started.");
    }

    public async Task StopAsync(CancellationToken ct = default)
    {
        Console.WriteLine($"[Engine] LoopEngine '{Id}' stopping...");
        for (var i = _runners.Count - 1; i >= 0; i--)
            await _runners[i].StopAsync(ct).ConfigureAwait(false);
        Console.WriteLine($"[Engine] LoopEngine '{Id}' stopped.");
    }
}