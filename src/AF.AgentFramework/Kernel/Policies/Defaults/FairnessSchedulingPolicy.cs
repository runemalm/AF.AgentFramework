namespace AgentFramework.Kernel.Policies.Defaults;

/// <summary>
/// A fairness-oriented scheduler that reduces the risk of one long-running or
/// high-utilization agent monopolizing kernel time.
/// <para>
/// The policy prefers agents that are:
///   - Not currently running
///   - Have queued work
///   - Have lower recent utilization
///   - Have smaller average queue length
/// </para>
/// 
/// The score function gives heavier penalty to agents with high utilization,
/// and a mild preference for those with non-empty but not huge queues.
/// Lower score = higher scheduling priority.
/// </summary>
public sealed class FairnessSchedulingPolicy : ISchedulingPolicy
{
    public AgentSelection? SelectNext(IEnumerable<IAgentView> agents, SchedulingContext context)
    {
        // Collect eligible agents
        var candidates = agents
            .Where(a => !a.IsRunning && a.QueueLength > 0)
            .Select(a => new
            {
                Agent = a,
                // Weighted heuristic: prefer lower utilization and modest queue pressure
                // Utilization penalty dominates (0.7 weight); queue length only lightly affects tie-breaking
                Score = (a.UtilizationPercent * 0.7) + (a.QueueLength * 0.1)
            })
            .OrderBy(x => x.Score)
            .ToList();

        if (candidates.Count == 0)
            return null;

        var selected = candidates.First().Agent;
        return new AgentSelection(selected.Id);
    }
}
