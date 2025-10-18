using AgentFramework.Tools.Contracts;
using AgentFramework.Tools.Registry;
using AgentFramework.Tools.Runtime;

namespace AgentFramework.Tools.Pipeline.Defaults;

public sealed class LocalExecutor : IExecutor
{
    private readonly IEnumerable<ILocalTool> _tools;
    private readonly Dictionary<string, ILocalTool> _lookup;

    public LocalExecutor(IEnumerable<ILocalTool> tools)
    {
        _tools = tools;
        _lookup = tools.ToDictionary(t => t.Name, StringComparer.OrdinalIgnoreCase);
    }

    public async Task<ExecutionOutcome> ExecuteAsync(
        ToolInvocation invocation,
        ToolDescriptor descriptor,
        CancellationToken cancellationToken)
    {
        if (_lookup.TryGetValue(invocation.ToolName, out var tool))
        {
            var result = await tool.ExecuteAsync(invocation.Input, cancellationToken).ConfigureAwait(false);
            return ExecutionOutcome.FromResult(result);
        }

        return ExecutionOutcome.FromError(new ToolError
        {
            Code = ToolErrorCode.ContractError,
            Subcode = "UnknownTool",
            Message = $"Local tool '{invocation.ToolName}' not found.",
            Origin = "local",
            IsRetryable = false
        });
    }
}
