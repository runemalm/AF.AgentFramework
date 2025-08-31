using AgentFramework.Kernel.Policies;

namespace AgentFramework.Kernel.Tests.Policies;

public class TimeoutPolicyDecoratorTests
{
    private sealed class DelayedPolicy : IPolicy
    {
        private readonly TimeSpan _delay;
        public readonly TaskCompletionSource<bool> CancelObserved = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public DelayedPolicy(TimeSpan delay) => _delay = delay;

        public async Task<Decision> DecideAsync(AgentContext ctx, CancellationToken ct = default)
        {
            try
            {
                await Task.Delay(_delay, ct);
                return new Decision(new Message("assistant", "done"), Array.Empty<ToolCall>());
            }
            catch (OperationCanceledException)
            {
                CancelObserved.TrySetResult(true);
                throw;
            }
        }
    }

    [Fact]
    public async Task Cancels_Inner_On_Timeout_And_Throws_TimeoutException()
    {
        var inner = new DelayedPolicy(TimeSpan.FromSeconds(5));
        var policy = new TimeoutPolicyDecorator(inner, timeout: TimeSpan.FromMilliseconds(150));

        var (agent, ctx) = new AgentBuilder().UsePolicy(policy).Build(new AgentId("t"));
        ctx.Conversation.Add(new Message("user", "go"));

        await Assert.ThrowsAsync<TimeoutException>(() => agent.StepAsync(ctx));

        // ensure inner observed cancellation
        Assert.True(await inner.CancelObserved.Task.WaitAsync(TimeSpan.FromSeconds(2)));
    }
}