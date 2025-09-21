namespace AgentFramework.Kernel.Policies;

public interface IBackpressurePolicy
{
    BackpressureDecision Evaluate(ClusterLoad load);
}
