using AgentFramework.Tools.Contracts;

namespace AgentFramework.Tools.Pipeline.Defaults;

/// <summary>
/// Default executor that indicates no provider has been bound for execution.
/// Useful placeholder until Local/MCP providers are integrated.
/// </summary>
public sealed class NotImplementedExecutor : IExecutor
{
    public Task<ExecutionOutcome> ExecuteAsync(
        ToolInvocation invocation,
        ToolDescriptor descriptor,
        CancellationToken cancellationToken)
    {
        var error = new ToolError
        {
            Code = ToolErrorCode.ExecutionError,
            Subcode = "NotImplemented",
            Message = $"No executor/provider is bound for tool '{descriptor.Name}'.",
            IsRetryable = false,
            Origin = descriptor.Origin
        };

        return Task.FromResult(new ExecutionOutcome { Error = error });
    }
}