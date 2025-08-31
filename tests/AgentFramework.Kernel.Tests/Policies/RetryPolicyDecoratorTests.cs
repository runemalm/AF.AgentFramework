using AgentFramework.Kernel.Policies;

namespace AgentFramework.Kernel.Tests.Policies;

public class RetryPolicyDecoratorTests
{
    private sealed class ThrowNTimesPolicy : IPolicy
    {
        private int _remaining;
        public int Attempts { get; private set; }
        public ThrowNTimesPolicy(int timesToThrow) => _remaining = timesToThrow;
        public Task<Decision> DecideAsync(AgentContext ctx, CancellationToken ct = default)
        {
            Attempts++;
            if (_remaining-- > 0) throw new HttpRequestException("transient");
            return Task.FromResult(new Decision(new Message("assistant", "ok"), Array.Empty<ToolCall>()));
        }
    }

    [Fact]
    public async Task Retries_On_Transient_Then_Succeeds()
    {
        var inner = new ThrowNTimesPolicy(timesToThrow: 2);
        var policy = new RetryPolicyDecorator(inner, maxAttempts: 3,
            shouldRetry: ex => ex is HttpRequestException);

        var (agent, ctx) = new AgentBuilder().UsePolicy(policy).Build(new AgentId("t"));
        ctx.Conversation.Add(new Message("user", "go"));

        var reply = await agent.StepAsync(ctx);

        Assert.Equal("ok", reply?.Content);
        Assert.Equal(3, inner.Attempts); // 2 throws + 1 success
    }

    [Fact]
    public async Task Does_Not_Retry_On_NonRetryable()
    {
        var inner = new ThrowNTimesPolicy(timesToThrow: 1);
        var policy = new RetryPolicyDecorator(inner, maxAttempts: 5,
            shouldRetry: ex => ex is InvalidOperationException); // HttpRequestException not retryable here

        var (agent, ctx) = new AgentBuilder().UsePolicy(policy).Build(new AgentId("t"));
        ctx.Conversation.Add(new Message("user", "go"));

        await Assert.ThrowsAsync<HttpRequestException>(() => agent.StepAsync(ctx));
        Assert.Equal(1, inner.Attempts);
    }

    [Fact]
    public async Task Does_Not_Retry_On_Cancellation()
    {
        var inner = new ThrowNTimesPolicy(timesToThrow: 10);
        var policy = new RetryPolicyDecorator(inner, maxAttempts: 5);

        var (agent, ctx0) = new AgentBuilder().UsePolicy(policy).Build(new AgentId("t"));
        ctx0.Conversation.Add(new Message("user", "go"));

        using var cts = new CancellationTokenSource();
        cts.Cancel();
        var ctx = new AgentContext(ctx0.AgentId, ctx0.Conversation, ctx0.Tools, ctx0.Memory, ctx0.Metadata, cts.Token);

        await Assert.ThrowsAsync<OperationCanceledException>(() => agent.StepAsync(ctx));
        Assert.Equal(0, inner.Attempts); // cancelled before call
    }
}
