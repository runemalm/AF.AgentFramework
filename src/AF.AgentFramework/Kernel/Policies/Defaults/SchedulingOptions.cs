namespace AgentFramework.Kernel.Policies.Defaults;

/// <summary>
/// Configuration options for scheduling policies controlling fairness and prioritization.
/// </summary>
/// <param name="UtilizationWeight">
/// Weight applied to penalize agents that have consumed more kernel CPU time recently
/// (as measured by <see cref="AgentSnapshot.UtilizationPercent"/>).
/// Higher values make the scheduler prefer less-utilized agents.
/// </param>
/// <param name="ExecutionTimeWeight">
/// Weight applied to penalize agents with longer average execution times per work item
/// (as measured by <see cref="AgentSnapshot.AvgExecutionMs"/>).
/// Used mainly by time-slice aware schedulers.
/// </param>
/// <param name="QueueLengthWeight">
/// Weight applied to favor agents with longer queues, ensuring backlog fairness.
/// Higher values give agents with larger queues a greater chance of being scheduled.
/// </param>
/// <param name="RandomJitterWeight">
/// Optional small weight for injecting controlled randomness to break ties and prevent
/// perfect oscillation patterns. Typical range: 0.0 – 0.05.
/// </param>
public sealed record SchedulingOptions(
    double UtilizationWeight = 0.7,
    double ExecutionTimeWeight = 0.5,
    double QueueLengthWeight = 0.1,
    double RandomJitterWeight = 0.0
)
{
    /// <summary>Default scheduling options with balanced weights.</summary>
    public static SchedulingOptions Default => new();
}
