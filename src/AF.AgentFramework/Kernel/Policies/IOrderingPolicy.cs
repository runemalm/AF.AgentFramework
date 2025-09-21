namespace AgentFramework.Kernel.Policies;

public interface IOrderingPolicy
{
    /// <summary>Return &lt;0 if a before b, 0 if equal, &gt;0 if after.</summary>
    int Compare(WorkItem a, WorkItem b);
}
