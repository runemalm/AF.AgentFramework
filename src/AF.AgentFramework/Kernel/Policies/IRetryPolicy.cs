namespace AgentFramework.Kernel.Policies;

public interface IRetryPolicy
{
    RetryDecision OnFailure(WorkItem item, Exception error, int attempt);
}
