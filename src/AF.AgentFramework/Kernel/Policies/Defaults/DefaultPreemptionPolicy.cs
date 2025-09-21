namespace AgentFramework.Kernel.Policies.Defaults;

public sealed class DefaultPreemptionPolicy : IPreemptionPolicy
{
    public PreemptionDecision ShouldPreempt(WorkItem incoming, RunningInvocation current)
        => PreemptionDecision.No; // Cooperative preemption disabled by default
}
