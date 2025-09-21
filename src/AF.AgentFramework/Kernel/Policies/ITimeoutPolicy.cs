namespace AgentFramework.Kernel.Policies;

public interface ITimeoutPolicy
{
    /// <summary>Return null for no timeout; otherwise used to cancel the agent context.</summary>
    TimeSpan? GetTimeout(WorkItem item);
}
