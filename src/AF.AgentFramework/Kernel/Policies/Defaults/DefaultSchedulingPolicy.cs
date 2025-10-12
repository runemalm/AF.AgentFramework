namespace AgentFramework.Kernel.Policies.Defaults;

/// <summary>
/// Simple fair round-robin scheduler across all agents.
/// Skips agents that are currently running or have empty queues.
/// </summary>
public sealed class DefaultSchedulingPolicy : ISchedulingPolicy
{
    private int _lastIndex = -1;

    public AgentSelection? SelectNext(IEnumerable<IAgentView> agents, SchedulingContext context)
    {
        var list = agents.ToList();
        if (list.Count == 0) return null;

        for (int i = 0; i < list.Count; i++)
        {
            _lastIndex = (_lastIndex + 1) % list.Count;
            var a = list[_lastIndex];

            if (!a.IsRunning && a.QueueLength > 0)
                return new AgentSelection(a.Id);
        }

        return null;
    }
}
