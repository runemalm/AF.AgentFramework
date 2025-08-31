using AgentFramework.Kernel.Policies;

namespace AgentFramework.Kernel.Tests.Policies;

public class RouterPolicyTests
{
    private sealed class ConstPolicy(string text) : IPolicy
    {
        public Task<Decision> DecideAsync(AgentContext ctx, CancellationToken ct = default)
            => Task.FromResult(new Decision(new Message("assistant", text), Array.Empty<ToolCall>()));
    }

    private static (Agent a, AgentContext c) Build(IPolicy p)
        => new AgentBuilder().UsePolicy(p).Build(new AgentId("t"));

    [Fact]
    public async Task Routes_By_Key()
    {
        var router = new RouterPolicy(
            routes: new Dictionary<string, IPolicy>
            {
                ["a"] = new ConstPolicy("A!"),
                ["b"] = new ConstPolicy("B!")
            },
            classify: ctx => ctx.Conversation[^1].Content.StartsWith("a", StringComparison.OrdinalIgnoreCase) ? "a" : "b"
        );

        var (agent, ctx) = Build(router);

        ctx.Conversation.Add(new Message("user", "alpha"));
        Assert.Equal("A!", (await agent.StepAsync(ctx))?.Content);

        ctx.Conversation.Add(new Message("user", "beta"));
        Assert.Equal("B!", (await agent.StepAsync(ctx))?.Content);
    }

    [Fact]
    public async Task Uses_Fallback_When_No_Route()
    {
        var router = new RouterPolicy(
            routes: new Dictionary<string, IPolicy>(),
            classify: _ => "unknown",
            fallback: new ConstPolicy("fallback")
        );

        var (agent, ctx) = Build(router);
        ctx.Conversation.Add(new Message("user", "x"));

        Assert.Equal("fallback", (await agent.StepAsync(ctx))?.Content);
    }
}