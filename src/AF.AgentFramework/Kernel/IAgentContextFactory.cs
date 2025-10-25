namespace AgentFramework.Kernel;

/// <summary>
/// Factory abstraction for creating <see cref="IAgentContext"/> instances.
/// </summary>
public interface IAgentContextFactory
{
    /// <summary>
    /// Creates a new <see cref="IAgentContext"/> for the specified work item.
    /// </summary>
    IAgentContext CreateContext(WorkItem item, CancellationToken cancellation);
}
