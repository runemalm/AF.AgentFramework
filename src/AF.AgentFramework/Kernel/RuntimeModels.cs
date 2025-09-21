namespace AgentFramework.Kernel;

public sealed record AgentRuntimeState(bool IsRunning, int QueueLength);
public sealed record RunningInvocation(WorkItem Item, DateTimeOffset StartedAt);
public sealed record ClusterLoad(int TotalQueued, int TotalRunning);
