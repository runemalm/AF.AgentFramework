namespace AgentFramework.Kernel.Policies.Defaults;

public sealed class DefaultRetryPolicy : IRetryPolicy
{
    private readonly RetryOptions _options;

    public DefaultRetryPolicy(RetryOptions? options = null)
        => _options = options ?? RetryOptions.Default;

    public RetryDecision OnFailure(WorkItem item, Exception error, int attempt)
    {
        // Don't retry cancellations (timeouts/preemption/shutdown should surface as canceled)
        if (error is OperationCanceledException) return new RetryDecision(false, Reason: "Canceled");

        if (attempt >= _options.MaxAttempts) return new RetryDecision(false, Reason: "MaxAttemptsReached");

        // Minimal backoff calc; jitter optional
        var delayMs = _options.BaseDelay.TotalMilliseconds <= 0 ? 250 : _options.BaseDelay.TotalMilliseconds;
        var pow = Math.Pow(2, Math.Max(0, attempt - 1));
        var next = TimeSpan.FromMilliseconds(Math.Min(_options.MaxDelay.TotalMilliseconds <= 0 ? 10_000 : _options.MaxDelay.TotalMilliseconds,
                                                     delayMs * pow));
        if (_options.UseJitter)
        {
            var rand = new Random();
            var jitter = rand.NextDouble() * 0.25 + 0.875; // ~[-12.5%, +12.5%]
            next = TimeSpan.FromMilliseconds(next.TotalMilliseconds * jitter);
        }
        return new RetryDecision(true, next);
    }
}
