namespace AgentFramework.Kernel.Features;

/// <summary>
/// Represents an extension capability attached to an <see cref="IAgentContext"/>.
/// Features are optional, composable modules that extend the agent environment.
/// </summary>
public interface IAgentFeature
{
    /// <summary>
    /// Called once when the feature is first attached to the context.
    /// May be used to bind the feature to other features or perform initialization.
    /// </summary>
    void Attach(IAgentContext context);
}
