namespace AgentFramework.Kernel.Tests;

using AgentFramework.Kernel;
using Xunit;

public class AgentSmokeTests
{
    [Fact]
    public async Task EchoPolicy_RepliesWithLastUserMessage()
    {
        var (agent, ctx) = new AgentBuilder().Build(new AgentId("test"));
        ctx.Conversation.Add(new Message("user", "Hello"));
        var reply = await agent.StepAsync(ctx);

        Assert.NotNull(reply);
        Assert.Equal("assistant", reply!.Role);
        Assert.Equal("Hello", reply.Content);
    }
}
