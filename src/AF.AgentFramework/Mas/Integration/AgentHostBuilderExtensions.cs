using AgentFramework.Hosting;
using AgentFramework.Mas.Features;
using Microsoft.Extensions.DependencyInjection;

namespace AgentFramework.Mas.Integration;

/// <summary>
/// Host builder extension methods for enabling the Multi-Agent System (MAS) capabilities.
/// Adds the Directory and Blackboard subsystems to the host.
/// </summary>
public static class AgentHostBuilderExtensions
{
    /// <summary>
    /// Enables the MAS subsystem for this host.
    /// Registers the Directory and Blackboard capabilities as shared instances,
    /// and adds them to the capability registry.
    /// </summary>
    public static IAgentHostBuilder AddMas(this IAgentHostBuilder builder)
    {
        if (builder is null)
            throw new ArgumentNullException(nameof(builder));

        if (builder is not IAgentCapabilityRegistrar registrar)
            throw new InvalidOperationException("Host builder does not support capability registration.");

        // Register shared implementations
        builder.Services.AddSingleton<IMasDirectory, MasDirectoryFeature>();
        builder.Services.AddSingleton<IMasBlackboard, MasBlackboardFeature>();

        // Delegate capability registration to module registrar
        MasCapabilityRegistrar.Register(registrar);

        return builder;
    }
}
