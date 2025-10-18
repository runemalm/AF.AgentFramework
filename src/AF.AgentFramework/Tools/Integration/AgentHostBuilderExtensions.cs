using Microsoft.Extensions.DependencyInjection;
using AgentFramework.Hosting;
using AgentFramework.Tools.Integration.Initialization;
using AgentFramework.Tools.Observability;
using AgentFramework.Tools.Pipeline;
using AgentFramework.Tools.Pipeline.Defaults;
using AgentFramework.Tools.Registry;

namespace AgentFramework.Tools.Integration;

/// <summary>
/// Extensions for wiring tool support into an <c>AgentHostBuilder</c>.
/// </summary>
public static class AgentHostBuilderExtensions
{
    /// <summary>
    /// Adds tool registry, pipeline, and observability defaults to the host builder's services.
    /// </summary>
    public static IAgentHostBuilder AddTools(this IAgentHostBuilder builder)
    {
        builder.Services.AddSingleton<IToolRegistry, InMemoryToolRegistry>();
        builder.Services.AddSingleton<IInputValidator, PassThroughInputValidator>();
        builder.Services.AddSingleton<IAuthorizer, AllowAllAuthorizer>();
        builder.Services.AddSingleton<IPolicyApplier, TimeoutOnlyPolicyApplier>();
        builder.Services.AddSingleton<IExecutor, LocalExecutor>();
        builder.Services.AddSingleton<IPostprocessor, PassThroughPostprocessor>();

        builder.Services.AddSingleton<IToolEventSink, NullToolEventSink>();
        builder.Services.AddSingleton<IToolTracer, NullToolTracer>();
        builder.Services.AddSingleton<IToolInvoker, ToolPipeline>();
        builder.Services.AddSingleton<IToolRegistryInitializer, ToolRegistryInitializer>();

        builder.AddHostService(() => new ToolRegistryInitializationService());

        builder.Services.AddSingleton<ToolSubsystemFactory>(sp => agentId =>
        {
            var registry = sp.GetRequiredService<IToolRegistry>();
            var invoker  = sp.GetRequiredService<IToolInvoker>();
            return new AgentContextTools(registry, invoker, new ToolBindingContext { AgentId = agentId });
        });

        return builder;
    }
}