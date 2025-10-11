namespace AgentFramework.Kernel.Diagnostics;

/// <summary>
/// Immutable snapshot of kernel state at a single point in time.
/// </summary>
public sealed record KernelSnapshot(
    int TotalAgents,
    int RunningAgents,
    int QueuedItems,
    IReadOnlyList<AgentSnapshot> Agents,
    DateTimeOffset Timestamp);

/// <summary>
/// Lightweight runtime info about one agent’s mailbox and status.
/// </summary>
public sealed record AgentSnapshot(
    string Id,
    int QueueLength,
    bool IsRunning);
