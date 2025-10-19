using Microsoft.Extensions.DependencyInjection;
using AgentFramework.Hosting;
using AgentFramework.Kernel.Diagnostics;
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
    /// Adds tool registry, pipeline, observability, and metrics support
    /// to the host builder's service collection.
    /// </summary>
    public static IAgentHostBuilder AddTools(this IAgentHostBuilder builder)
    {
        // Core pipeline services
        builder.Services.AddSingleton<IInputValidator, PassThroughInputValidator>();
        builder.Services.AddSingleton<IAuthorizer, AllowAllAuthorizer>();
        builder.Services.AddSingleton<IPolicyApplier, TimeoutOnlyPolicyApplier>();
        builder.Services.AddSingleton<IExecutor, LocalExecutor>();
        builder.Services.AddSingleton<IPostprocessor, PassThroughPostprocessor>();

        // --- Observability and metrics ---
        builder.Services.AddSingleton<ToolsMetricsProvider>();
        builder.Services.AddSingleton<IToolEventSink>(sp => sp.GetRequiredService<ToolsMetricsProvider>());
        builder.Services.AddSingleton<IAgentMetricsProvider>(sp => sp.GetRequiredService<ToolsMetricsProvider>());
        builder.Services.AddSingleton<IToolTracer, NullToolTracer>();

        // --- Registry ---
        builder.Services.AddSingleton<IToolRegistry, InMemoryToolRegistry>();
        builder.Services.AddSingleton<IToolRegistryInitializer, ToolRegistryInitializer>();
        builder.AddHostService(() => new ToolRegistryInitializationService());

        // --- Invoker with instrumentation ---
        builder.Services.AddSingleton<IToolInvoker>(sp =>
            new InstrumentedToolInvoker(
                new ToolPipeline(
                    sp.GetRequiredService<IToolRegistry>(),
                    sp.GetRequiredService<IInputValidator>(),
                    sp.GetRequiredService<IAuthorizer>(),
                    sp.GetRequiredService<IPolicyApplier>(),
                    sp.GetRequiredService<IExecutor>(),
                    sp.GetRequiredService<IPostprocessor>()
                ),
                sp.GetRequiredService<IToolEventSink>(),
                sp.GetRequiredService<IToolTracer>(),
                sp.GetRequiredService<IToolRegistry>())
        );

        // --- Subsystem factory (per-agent context) ---
        builder.Services.AddSingleton<ToolSubsystemFactory>(sp => agentId =>
        {
            var registry = sp.GetRequiredService<IToolRegistry>();
            var invoker  = sp.GetRequiredService<IToolInvoker>();
            return new AgentContextTools(
                registry,
                invoker,
                new ToolBindingContext { AgentId = agentId }
            );
        });

        return builder;
    }
}
