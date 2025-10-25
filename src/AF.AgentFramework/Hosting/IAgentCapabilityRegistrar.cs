namespace AgentFramework.Hosting;

/// <summary>
/// Contract used by framework modules to register agent capabilities (features)
/// that will be attached to every <see cref="IAgentContext"/> at runtime.
/// </summary>
public interface IAgentCapabilityRegistrar
{
    /// <summary>
    /// Registers a capability (feature) by its interface and implementation type.
    /// Registered capabilities are resolved from the host’s DI container
    /// and attached to each agent context created by the kernel.
    /// </summary>
    /// <param name="interfaceType">The capability interface type.</param>
    /// <param name="implementationType">The feature implementation type.</param>
    void RegisterCapability(Type interfaceType, Type implementationType);
}
