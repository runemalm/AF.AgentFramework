namespace AgentFramework.Kernel.Tests;

public class MiddlewareOrderTests
{
    private sealed class TraceMw(string name, List<string> trace) : IAgentMiddleware
    {
        public Task InvokeAsync(AgentContext context, Func<Task> next)
        {
            trace.Add($"pre:{name}");
            return next().ContinueWith(t =>
            {
                trace.Add($"post:{name}");
                if (t.IsFaulted) throw t.Exception!.InnerException!;
            });
        }
    }

    private sealed class SimplePolicy : IPolicy
    {
        public Task<Decision> DecideAsync(AgentContext context, CancellationToken ct = default)
            => Task.FromResult(new Decision(new Message("assistant", "ok"), Array.Empty<ToolCall>()));
    }

    [Fact]
    public async Task Middleware_Executes_In_Declared_Order_And_Unwinds_Reverse()
    {
        var trace = new List<string>();
        var builder = new AgentBuilder()
            .UseMiddleware(new TraceMw("A", trace))
            .UseMiddleware(new TraceMw("B", trace))
            .UsePolicy(new SimplePolicy());

        var (agent, ctx) = builder.Build(new AgentId("t"));
        ctx.Conversation.Add(new Message("user", "hi"));

        var reply = await agent.StepAsync(ctx);

        Assert.Equal(new[] { "pre:A", "pre:B", "post:B", "post:A" }, trace);
        Assert.Equal("ok", reply?.Content);
    }
}