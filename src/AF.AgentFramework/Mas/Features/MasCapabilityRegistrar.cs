using AgentFramework.Hosting;

namespace AgentFramework.Mas.Features;

/// <summary>
/// Internal helper that registers all MAS-related capabilities
/// (Directory, Blackboard, etc.) with the host builder's capability registrar.
/// </summary>
internal static class MasCapabilityRegistrar
{
    /// <summary>
    /// Registers all MAS subsystem capabilities with the given registrar.
    /// </summary>
    public static void Register(IAgentCapabilityRegistrar registrar)
    {
        if (registrar is null)
            throw new ArgumentNullException(nameof(registrar));

        registrar.RegisterCapability(typeof(IMasDirectory), typeof(MasDirectoryFeature));
        registrar.RegisterCapability(typeof(IMasBlackboard), typeof(MasBlackboardFeature));
    }
}
