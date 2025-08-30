namespace AgentFramework.Agent.Policies;

/// <summary>
/// Decorator that enforces a maximum duration for an inner policy's DecideAsync.
/// If the timeout elapses, a TimeoutException is thrown (and the inner call is cancelled).
/// </summary>
public sealed class TimeoutPolicyDecorator : IPolicy
{
    private readonly IPolicy _inner;
    private readonly TimeSpan _timeout;
    private readonly Action<TimeSpan, AgentContext>? _onTimeout;

    public TimeoutPolicyDecorator(IPolicy inner, TimeSpan timeout, Action<TimeSpan, AgentContext>? onTimeout = null)
    {
        if (timeout <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(timeout), "Timeout must be > 0.");
        _inner = inner ?? throw new ArgumentNullException(nameof(inner));
        _timeout = timeout;
        _onTimeout = onTimeout;
    }

    public async Task<Decision> DecideAsync(AgentContext context, CancellationToken ct = default)
    {
        if (ct.IsCancellationRequested) ct.ThrowIfCancellationRequested();

        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        var innerTask = _inner.DecideAsync(context, linkedCts.Token);

        // Race inner vs timeout delay; if delay wins, cancel inner & throw TimeoutException
        var delayTask = Task.Delay(_timeout, ct);

        var completed = await Task.WhenAny(innerTask, delayTask).ConfigureAwait(false);
        if (completed == innerTask)
        {
            // Propagate inner completion/exception
            return await innerTask.ConfigureAwait(false);
        }

        // Timeout path
        try { linkedCts.Cancel(); } catch { /* best effort */ }
        _onTimeout?.Invoke(_timeout, context);
        throw new TimeoutException($"Policy timed out after {_timeout}.");
    }
}