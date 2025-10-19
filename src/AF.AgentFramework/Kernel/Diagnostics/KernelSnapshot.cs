namespace AgentFramework.Kernel.Diagnostics;

/// <summary>
/// Immutable snapshot of kernel state at a single point in time.
/// </summary>
public sealed record KernelSnapshot(
    int TotalAgents,
    int RunningAgents,
    int QueuedItems,
    int RejectedItems,              // total across all agents
    int TotalHandledItems,
    double ThroughputPerSecond,     // moving average of completed WorkItems/sec
    IReadOnlyList<AgentSnapshot> Agents,
    DateTimeOffset Timestamp,
    int WorkerCount
);

/// <summary>
/// Lightweight runtime info about one agent’s mailbox and status.
/// </summary>
public sealed record AgentSnapshot(
    string Id,
    int QueueLength,
    bool IsRunning,
    int TotalHandled, // completed items since kernel start
    int Rejected, // admission rejections for this agent
    double AvgExecutionMs, // moving average execution duration
    double QueueGrowthRate, // items per second delta
    double UtilizationPercent, // (activeTime / totalRuntime) * 100
    IReadOnlyDictionary<string, object>? Metrics = null // optional extension data
)
{
    /// <summary>
    /// Creates a copy of this snapshot with merged metrics.
    /// </summary>
    public AgentSnapshot WithMetrics(IReadOnlyDictionary<string, object> metrics)
        => this with { Metrics = metrics };
}
