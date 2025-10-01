using AgentFramework.Tools.Contracts;

namespace AgentFramework.Tools.Pipeline;

/// <summary>
/// Entry point for invoking tools through the standardized pipeline.
/// </summary>
public interface IToolInvoker
{
    /// <summary>
    /// Invokes the specified tool using the configured pipeline stages.
    /// Implementations must always return a <see cref="ToolResult"/>; exceptions are reserved for catastrophic failures.
    /// </summary>
    Task<ToolResult> InvokeAsync(ToolInvocation invocation, CancellationToken cancellationToken = default);
}
