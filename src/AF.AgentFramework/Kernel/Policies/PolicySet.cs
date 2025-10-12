namespace AgentFramework.Kernel.Policies;

public sealed class PolicySet
{
    public IAdmissionPolicy? Admission { get; init; }
    public IPreemptionPolicy? Preemption { get; init; }
    public IRetryPolicy? Retry { get; init; }
    public ITimeoutPolicy? Timeout { get; init; }
    public IOrderingPolicy? Ordering { get; init; }
    public IBackpressurePolicy? Backpressure { get; init; }
    public ISchedulingPolicy? Scheduling { get; init; }
}
