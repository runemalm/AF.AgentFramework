namespace AgentFramework.Kernel.Policies;

/// <summary>
/// Determines which agent should receive execution time next.
/// </summary>
public interface ISchedulingPolicy
{
    /// <summary>
    /// Selects the next agent to schedule for execution.
    /// Returns null if no agent is eligible.
    /// </summary>
    AgentSelection? SelectNext(IEnumerable<IAgentView> agents, SchedulingContext context);
}

/// <summary>
/// Minimal read-only view of agent state for scheduling decisions.
/// </summary>
public interface IAgentView
{
    string Id { get; }
    int QueueLength { get; }
    bool IsRunning { get; }
    double UtilizationPercent { get; }
}

/// <summary>
/// Context snapshot for global scheduling.
/// </summary>
public readonly record struct SchedulingContext(
    int TotalRunning,
    int TotalQueued,
    DateTimeOffset Now
);

/// <summary>
/// Result of a scheduling decision.
/// </summary>
public readonly record struct AgentSelection(string AgentId);
