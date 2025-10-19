using AgentFramework.Tools.Contracts;
using AgentFramework.Tools.Pipeline;
using AgentFramework.Tools.Registry;

namespace AgentFramework.Tools.Integration;

/// <summary>
/// Facade exposed on IAgentContext to allow agents to discover and invoke tools.
/// </summary>
public sealed class AgentContextTools
{
    private readonly IToolRegistry _registry;
    private readonly IToolInvoker _invoker;
    private readonly ToolBindingContext _bindingContext;

    public AgentContextTools(
        IToolRegistry registry,
        IToolInvoker invoker,
        ToolBindingContext bindingContext)
    {
        _registry = registry;
        _invoker = invoker;
        _bindingContext = bindingContext;
    }

    /// <summary>
    /// List tool descriptors available to this agent.
    /// </summary>
    public IReadOnlyList<ToolDescriptor> ListTools() => _registry.List(_bindingContext);

    /// <summary>
    /// Invoke a tool through the pipeline.
    /// </summary>
    public Task<ToolResult> InvokeAsync(ToolInvocation invocation, CancellationToken cancellationToken = default)
    {
        var enriched = invocation with { AgentId = _bindingContext.AgentId };
        return _invoker.InvokeAsync(enriched, cancellationToken);
    }
}
