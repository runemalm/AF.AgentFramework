using AgentFramework.Kernel;
using AgentFramework.Tools.Contracts;

namespace HelloKernel;

/// <summary>
/// Demonstrates invoking a tool through the Tools subsystem.
/// Each tick calls the "local::echo" tool and logs its result.
/// </summary>
sealed class HelloToolsAgent : IAgent
{
    public string Id { get; } = "hello-tools";
    private int _loopCount;

    public async Task HandleAsync(WorkItem item, IAgentContext ctx)
    {
        _loopCount++;
        ctx.Trace($"[Agent] Loop #{_loopCount} — invoking tool...");

        if (ctx.Tools is null)
        {
            ctx.Trace("[Agent] Tools subsystem not available in this host.");
            return;
        }

        var invocation = new ToolInvocation
        {
            ToolName = "local::echo",
            Input = new { message = $"Hello from {_loopCount} at {DateTimeOffset.UtcNow:T}" },
            CorrelationId = item.Id,
        };

        var result = await ctx.Tools.InvokeAsync(invocation, ctx.Cancellation);

        if (result.Status == ToolResultStatus.Ok)
        {
            ctx.Trace($"[Agent] Tool OK → {System.Text.Json.JsonSerializer.Serialize(result.Output)}");
        }
        else
        {
            ctx.Trace($"[Agent] Tool ERROR → {result.Error?.Code}:{result.Error?.Subcode} {result.Error?.Message}");
        }

        // small pause before next tick
        var delay = 200 + ctx.Random.Next(300);
        await Task.Delay(delay, ctx.Cancellation);
    }
}
