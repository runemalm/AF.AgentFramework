namespace AgentFramework.Kernel;

public interface IAgentKernel
{
    KernelStatus Status { get; }

    Task StartAsync(CancellationToken ct = default);
    Task StopAsync(CancellationToken ct = default);

    /// <summary>
    /// Runs exactly one MAPE-K iteration (Monitor→Analyze→Plan→Execute→Knowledge).
    /// Non-blocking: returns after the single tick completes (successfully or with error).
    /// </summary>
    Task<AgentTickResult> TickAsync(CancellationToken ct = default);
}