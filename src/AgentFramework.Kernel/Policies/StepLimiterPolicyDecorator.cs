namespace AgentFramework.Kernel.Policies;

/// <summary>
/// Decorator that caps how many times DecideAsync can run per conversation/session.
/// Count is tracked in AgentContext.Metadata under a configurable key.
/// On limit, either throws or returns a final assistant message (configurable).
/// </summary>
public sealed class StepLimiterPolicyDecorator : IPolicy
{
    public enum OnLimitBehavior { Throw, ReturnFinalMessage }

    private readonly IPolicy _inner;
    private readonly int _maxDecisions;
    private readonly string _counterKey;
    private readonly OnLimitBehavior _behavior;
    private readonly string _finalMessage;

    public StepLimiterPolicyDecorator(
        IPolicy inner,
        int maxDecisions = 16,
        string? counterKey = null,
        OnLimitBehavior behavior = OnLimitBehavior.ReturnFinalMessage,
        string finalMessage = "Stopping: step limit reached.")
    {
        if (maxDecisions < 1) throw new ArgumentOutOfRangeException(nameof(maxDecisions), "Must be >= 1.");
        _inner = inner ?? throw new ArgumentNullException(nameof(inner));
        _maxDecisions = maxDecisions;
        _counterKey = counterKey ?? "agent.stepLimiter.count";
        _behavior = behavior;
        _finalMessage = finalMessage;
    }

    public Task<Decision> DecideAsync(AgentContext context, CancellationToken ct = default)
    {
        var count = context.Metadata.Get<int?>(_counterKey) ?? 0;
        if (count >= _maxDecisions)
        {
            return _behavior == OnLimitBehavior.Throw
                ? Task.FromException<Decision>(new InvalidOperationException($"Step limit of {_maxDecisions} reached."))
                : Task.FromResult(new Decision(new Message("assistant", _finalMessage), Array.Empty<ToolCall>()));
        }

        context.Metadata.Set(_counterKey, count + 1);
        return _inner.DecideAsync(context, ct);
    }
}
