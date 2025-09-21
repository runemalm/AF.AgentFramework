namespace AgentFramework.Kernel;

public interface IKernel
{
    Task StartAsync(CancellationToken ct = default);
    Task StopAsync(CancellationToken ct = default);

    /// <summary>Engines submit work here. At-least-once within-process.</summary>
    ValueTask EnqueueAsync(WorkItem item, CancellationToken ct = default);
}
