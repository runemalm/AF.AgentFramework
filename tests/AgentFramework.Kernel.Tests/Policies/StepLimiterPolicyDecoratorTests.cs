using AgentFramework.Kernel.Policies;

namespace AgentFramework.Kernel.Tests.Policies;

public class StepLimiterPolicyDecoratorTests
{
    private sealed class EchoPolicy : IPolicy
    {
        public int Calls { get; private set; }
        public Task<Decision> DecideAsync(AgentContext ctx, CancellationToken ct = default)
        {
            Calls++;
            return Task.FromResult(new Decision(new Message("assistant", $"ok#{Calls}"), Array.Empty<ToolCall>()));
        }
    }

    [Fact]
    public async Task Returns_Final_Message_When_Limit_Reached()
    {
        var inner = new EchoPolicy();
        var policy = new StepLimiterPolicyDecorator(inner, maxDecisions: 2,
            behavior: StepLimiterPolicyDecorator.OnLimitBehavior.ReturnFinalMessage,
            finalMessage: "limit!");

        var (agent, ctx) = new AgentBuilder().UsePolicy(policy).Build(new AgentId("t"));
        ctx.Conversation.Add(new Message("user", "go"));

        var r1 = await agent.StepAsync(ctx);
        var r2 = await agent.StepAsync(ctx);
        var r3 = await agent.StepAsync(ctx);

        Assert.Equal("ok#1", r1?.Content);
        Assert.Equal("ok#2", r2?.Content);
        Assert.Equal("limit!", r3?.Content);
        Assert.Equal(2, inner.Calls);
    }

    [Fact]
    public async Task Throws_When_Behavior_Is_Throw()
    {
        var inner = new EchoPolicy();
        var policy = new StepLimiterPolicyDecorator(inner, maxDecisions: 1,
            behavior: StepLimiterPolicyDecorator.OnLimitBehavior.Throw);

        var (agent, ctx) = new AgentBuilder().UsePolicy(policy).Build(new AgentId("t"));
        ctx.Conversation.Add(new Message("user", "go"));

        _ = await agent.StepAsync(ctx); // first ok
        await Assert.ThrowsAsync<InvalidOperationException>(() => agent.StepAsync(ctx));
    }
}