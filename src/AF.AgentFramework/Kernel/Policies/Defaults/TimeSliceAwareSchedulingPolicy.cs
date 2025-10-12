namespace AgentFramework.Kernel.Policies.Defaults;

/// <summary>
/// Scheduling policy that balances fairness and time-slice efficiency.
/// Agents with longer average execution times or high utilization are
/// penalized, while agents with shorter tasks or lighter load are favored.
/// </summary>
public sealed class TimeSliceAwareSchedulingPolicy : ISchedulingPolicy
{
    private readonly SchedulingOptions _options;
    private readonly Random _rand = new();

    public TimeSliceAwareSchedulingPolicy(SchedulingOptions? options = null)
    {
        _options = options ?? SchedulingOptions.Default;
    }

    public AgentSelection? SelectNext(IEnumerable<IAgentView> agents, SchedulingContext context)
    {
        var now = context.Now;
        var candidates = agents
            .Where(a => !a.IsRunning && a.QueueLength > 0)
            .Select(a =>
            {
                // Try to read AvgExecutionMs if available (extended view or snapshot)
                double avgMs = 0;
                if (a is IExtendedAgentView ext) avgMs = ext.AvgExecutionMs;

                // Weighted score
                var score =
                    (a.UtilizationPercent * _options.UtilizationWeight) +
                    (avgMs * _options.ExecutionTimeWeight / 100.0) + // scale to same range
                    (a.QueueLength * _options.QueueLengthWeight) +
                    (_options.RandomJitterWeight > 0 ? _rand.NextDouble() * _options.RandomJitterWeight : 0);

                return new { Agent = a, Score = score };
            })
            .OrderBy(x => x.Score)
            .ToList();

        if (candidates.Count == 0)
            return null;

        return new AgentSelection(candidates.First().Agent.Id);
    }
}

/// <summary>
/// Optional extended agent view providing average execution time for time-slice aware schedulers.
/// </summary>
public interface IExtendedAgentView : IAgentView
{
    double AvgExecutionMs { get; }
}
