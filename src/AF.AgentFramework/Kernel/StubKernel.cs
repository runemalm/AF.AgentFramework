namespace AgentFramework.Kernel;

/// <summary>
/// Temporary stub Kernel: logs enqueues; no dispatch yet.
/// </summary>
public sealed class StubKernel : IKernel
{
    private readonly KernelOptions _options;

    public StubKernel(KernelOptions options) => _options = options;

    public Task StartAsync(CancellationToken ct = default)
    {
        Console.WriteLine("[Kernel] Started.");
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken ct = default)
    {
        Console.WriteLine("[Kernel] Stopped.");
        return Task.CompletedTask;
    }

    public ValueTask EnqueueAsync(WorkItem item, CancellationToken ct = default)
    {
        Console.WriteLine($"[Kernel] Enqueued: Agent={item.AgentId}, Engine={item.EngineId}, Kind={item.Kind}, Id={item.Id}");
        return ValueTask.CompletedTask;
    }
}
