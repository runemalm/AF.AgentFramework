namespace AgentFramework.Kernel.Routing;

/// <summary>
/// Defines how incoming perceptual work items are distributed to agents.
/// </summary>
public interface IStimulusRouter
{
    /// <summary>
    /// Routes an incoming percept work item to one or more target agents.
    /// </summary>
    Task RouteAsync(
        WorkItem percept,
        IKernel kernel,
        IReadOnlyCollection<string> attachedAgents,
        CancellationToken ct = default);
}
