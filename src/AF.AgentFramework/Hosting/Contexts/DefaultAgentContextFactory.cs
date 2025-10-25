using AgentFramework.Kernel;
using AgentFramework.Kernel.Features;
using AgentFramework.Tools.Integration;

namespace AgentFramework.Hosting.Contexts;

/// <summary>
/// Default implementation of <see cref="IAgentContextFactory"/>.
/// Responsible for constructing <see cref="IAgentContext"/> instances and
/// attaching all registered features defined by <see cref="FeatureRegistration"/> metadata.
/// </summary>
internal sealed class DefaultAgentContextFactory : IAgentContextFactory
{
    private readonly IServiceProvider _sp;
    private readonly IReadOnlyList<FeatureRegistration> _registrations;
    private readonly ToolSubsystemFactory? _toolFactory;

    public DefaultAgentContextFactory(
        IServiceProvider serviceProvider,
        IReadOnlyList<FeatureRegistration> registrations,
        ToolSubsystemFactory? toolFactory)
    {
        _sp = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _registrations = registrations ?? throw new ArgumentNullException(nameof(registrations));
        _toolFactory = toolFactory;
    }

    public IAgentContext CreateContext(WorkItem item, CancellationToken cancellation)
    {
        ArgumentNullException.ThrowIfNull(item);

        // Optional tools subsystem
        var tools = _toolFactory?.Invoke(item.AgentId);

        // Create the base context
        var ctx = new AgentContext(
            item.AgentId,
            item.EngineId,
            item.Id,
            item.CorrelationId,
            cancellation,
            randomSeed: item.Id.GetHashCode(),
            knowledge: null,
            tools: tools);

        // Attach all registered features defined for this host
        foreach (var reg in _registrations)
        {
            var instance = _sp.GetService(reg.InterfaceType) as IAgentFeature;
            if (instance is null)
            {
                throw new InvalidOperationException(
                    $"Feature '{reg.InterfaceType.Name}' could not be resolved. " +
                    $"Ensure '{reg.ImplementationType.Name}' is registered in the DI container.");
            }

            // Initialize feature for this agent context
            instance.Attach(ctx);
            ctx.Features.Add(reg.InterfaceType, instance);
            ctx.Trace($"[Factory] Attached feature {reg.InterfaceType.Name}");
        }

        // Helpful for observability
        Console.WriteLine($"[ContextFactory] Created context for agent '{item.AgentId}' " +
                          $"with features: {ctx.Features}");

        return ctx;
    }
}
