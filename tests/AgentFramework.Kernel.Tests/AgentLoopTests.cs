namespace AgentFramework.Kernel.Tests;

public class AgentLoopTests
{
    private sealed class ThrowingPolicy : IPolicy
    {
        public Task<Decision> DecideAsync(AgentContext ctx, CancellationToken ct = default)
            => throw new InvalidOperationException("boom");
    }

    private sealed class CancellablePolicy : IPolicy
    {
        public Task<Decision> DecideAsync(AgentContext ctx, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();
            return Task.FromResult(new Decision(new Message("assistant", "should not reach"), Array.Empty<ToolCall>()));
        }
    }

    [Fact]
    public async Task StepAsync_Propagates_Exception_From_Policy()
    {
        var (agent, ctx) = new AgentBuilder().UsePolicy(new ThrowingPolicy()).Build(new AgentId("t"));
        ctx.Conversation.Add(new Message("user", "go"));

        await Assert.ThrowsAsync<InvalidOperationException>(() => agent.StepAsync(ctx));
    }

    [Fact]
    public async Task StepAsync_Respects_Cancellation_Token()
    {
        var (agent, baseCtx) = new AgentBuilder().UsePolicy(new CancellablePolicy()).Build(new AgentId("t"));
        baseCtx.Conversation.Add(new Message("user", "go"));

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var ctx = new AgentContext(baseCtx.AgentId, baseCtx.Conversation, baseCtx.Tools, baseCtx.Memory, baseCtx.Metadata, cts.Token);

        await Assert.ThrowsAsync<OperationCanceledException>(() => agent.StepAsync(ctx));
    }
}