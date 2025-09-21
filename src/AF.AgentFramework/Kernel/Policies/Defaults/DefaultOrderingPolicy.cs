namespace AgentFramework.Kernel.Policies.Defaults;

public sealed class DefaultOrderingPolicy : IOrderingPolicy
{
    public int Compare(WorkItem a, WorkItem b)
    {
        // Higher priority first
        var byPriority = b.Priority.CompareTo(a.Priority);
        if (byPriority != 0) return byPriority;

        // TODO: Kernel will likely stamp enqueue time; until then, fallback to Id compare for stability.
        // When enqueue timestamp exists, replace this with timestamp ascending.
        var byDeadline = CompareDeadline(a, b);
        if (byDeadline != 0) return byDeadline;

        return string.CompareOrdinal(a.Id, b.Id);
    }

    private static int CompareDeadline(WorkItem a, WorkItem b)
    {
        if (a.Deadline is null && b.Deadline is null) return 0;
        if (a.Deadline is null) return 1;
        if (b.Deadline is null) return -1;
        return a.Deadline.Value.CompareTo(b.Deadline.Value);
    }
}
