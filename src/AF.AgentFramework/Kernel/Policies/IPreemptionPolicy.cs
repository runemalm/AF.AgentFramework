namespace AgentFramework.Kernel.Policies;

public interface IPreemptionPolicy
{
    /// <summary>Decide if the incoming item may preempt the currently running one (same agent).</summary>
    PreemptionDecision ShouldPreempt(WorkItem incoming, RunningInvocation current);
}
