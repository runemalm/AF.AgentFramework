using AgentFramework.Tools.Contracts;
using AgentFramework.Tools.Runtime;

namespace HelloKernel.Tools;

public sealed class LocalEchoTool : ILocalTool
{
    public string Name => "local::echo";
    public string Version => "1.0.0";

    public ToolContract Contract => new()
    {
        InputSchema = new { message = "string" },
        OutputSchema = new { echoed = "string" }
    };

    public Task<ToolResult> ExecuteAsync(object? input, CancellationToken cancellationToken)
    {
        var msg = input?.GetType().GetProperty("message")?.GetValue(input)?.ToString() ?? "(no message)";
        return Task.FromResult(new ToolResult
        {
            Status = ToolResultStatus.Ok,
            Output = new { echoed = msg },
            Origin = "local"
        });
    }
}
