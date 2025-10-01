using AgentFramework.Tools.Contracts;

namespace AgentFramework.Tools.Pipeline;

/// <summary>
/// Executes the actual tool logic via a provider (Local, MCP, etc.).
/// In this slice, the default executor returns a NotImplemented error.
/// </summary>
public interface IExecutor
{
    Task<ExecutionOutcome> ExecuteAsync(
        ToolInvocation invocation,
        ToolDescriptor descriptor,
        CancellationToken cancellationToken);
}

/// <summary>
/// Result of an execution attempt.
/// Exactly one of <see cref="Result"/> or <see cref="Error"/> will be non-null.
/// </summary>
public sealed record class ExecutionOutcome
{
    public ToolResult? Result { get; init; }
    public ToolError? Error { get; init; }
}