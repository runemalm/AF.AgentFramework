using AgentFramework.Tools.Pipeline;
using AgentFramework.Tools.Pipeline.Defaults;
using AgentFramework.Tools.Registry;

namespace AgentFramework.Tools.Integration;

/// <summary>
/// Minimal bootstrap helper to wire up the default tools pipeline and registry
/// without taking a dependency on a specific DI container or host builder.
/// Hosts can replace any component after calling this.
/// </summary>
public static class ToolsBootstrap
{
    /// <summary>
    /// Creates a default in-memory registry and a basic pipeline with pass-through validation,
    /// allow-all authorization, timeout-only policy, not-implemented executor, and pass-through postprocessor.
    /// Observability is wired to no-op sinks by default.
    /// </summary>
    public static (IToolRegistry Registry, IToolInvoker Invoker) CreateDefaults()
    {
        var registry = new InMemoryToolRegistry();

        // Default pipeline stages
        IInputValidator validator = new PassThroughInputValidator();
        IAuthorizer authorizer = new AllowAllAuthorizer();
        IPolicyApplier policy = new TimeoutOnlyPolicyApplier();
        IExecutor executor = new NotImplementedExecutor();
        IPostprocessor postprocessor = new PassThroughPostprocessor();

        // Compose the pipeline
        IToolInvoker invoker = new ToolPipeline(
            registry,
            validator,
            authorizer,
            policy,
            executor,
            postprocessor);

        return (registry, invoker);
    }

    /// <summary>
    /// Convenience to construct the facade normally exposed on an agent context.
    /// </summary>
    public static AgentContextTools CreateAgentContextTools(string agentId)
    {
        var (registry, invoker) = CreateDefaults();
        var binding = new ToolBindingContext { AgentId = agentId };
        return new AgentContextTools(registry, invoker, binding);
    }
}