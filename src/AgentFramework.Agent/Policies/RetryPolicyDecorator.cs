namespace AgentFramework.Agent.Policies;

/// <summary>
/// Decorator that retries an inner IPolicy.DecideAsync when it throws,
/// using configurable backoff and exception filtering.
/// NOTE: This retries only the policy's decision phase. Tool invocation retries
/// should be handled via middleware or a separate tool-level retry decorator.
/// </summary>
public sealed class RetryPolicyDecorator : IPolicy
{
    public enum BackoffStrategy
    {
        Constant,
        Linear,
        Exponential,
        ExponentialJitter
    }

    private readonly IPolicy _inner;
    private readonly int _maxAttempts;
    private readonly TimeSpan _baseDelay;
    private readonly TimeSpan _maxDelay;
    private readonly BackoffStrategy _strategy;
    private readonly Func<Exception, bool> _shouldRetry;
    private readonly Action<Exception, int, TimeSpan, AgentContext>? _onRetry;
    private readonly Random _rng = new();

    public RetryPolicyDecorator(
        IPolicy inner,
        int maxAttempts = 3,
        TimeSpan? baseDelay = null,
        TimeSpan? maxDelay = null,
        BackoffStrategy strategy = BackoffStrategy.ExponentialJitter,
        Func<Exception, bool>? shouldRetry = null,
        Action<Exception, int, TimeSpan, AgentContext>? onRetry = null)
    {
        if (inner is null) throw new ArgumentNullException(nameof(inner));
        if (maxAttempts < 1) throw new ArgumentOutOfRangeException(nameof(maxAttempts), "maxAttempts must be >= 1");

        _inner = inner;
        _maxAttempts = maxAttempts;
        _baseDelay = baseDelay ?? TimeSpan.FromMilliseconds(200);
        _maxDelay = maxDelay ?? TimeSpan.FromSeconds(5);
        _strategy = strategy;
        _shouldRetry = shouldRetry ?? DefaultShouldRetry;
        _onRetry = onRetry;
    }

    public async Task<Decision> DecideAsync(AgentContext context, CancellationToken ct = default)
    {
        if (ct.IsCancellationRequested) ct.ThrowIfCancellationRequested();

        Exception? last = null;

        for (int attempt = 1; attempt <= _maxAttempts; attempt++)
        {
            try
            {
                return await _inner.DecideAsync(context, ct).ConfigureAwait(false);
            }
            catch (Exception ex) when (IsRetryable(ex) && attempt < _maxAttempts && !ct.IsCancellationRequested)
            {
                last = ex;
                var delay = ComputeDelay(attempt);
                _onRetry?.Invoke(ex, attempt, delay, context);

                try
                {
                    if (delay > TimeSpan.Zero)
                        await Task.Delay(delay, ct).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    throw; // honor cancellation during backoff
                }
            }
            catch
            {
                throw; // non-retryable or final attempt throws immediately
            }
        }

        // If we exit the loop without returning, rethrow the last retryable exception
        // (shouldn't happen because the loop throws on final attempt)
        throw last ?? new InvalidOperationException("RetryPolicyDecorator reached an unexpected state.");
    }

    private bool IsRetryable(Exception ex)
    {
        if (ex is OperationCanceledException || ex is TaskCanceledException)
            return false; // honor cancellation
        return _shouldRetry(ex);
    }

    private TimeSpan ComputeDelay(int attempt)
    {
        // attempt is 1-based for the first failure
        double baseMs = _baseDelay.TotalMilliseconds;
        double maxMs = _maxDelay.TotalMilliseconds;
        double ms = baseMs;

        switch (_strategy)
        {
            case BackoffStrategy.Constant:
                ms = baseMs;
                break;

            case BackoffStrategy.Linear:
                ms = baseMs * attempt;
                break;

            case BackoffStrategy.Exponential:
                ms = baseMs * Math.Pow(2, attempt - 1);
                break;

            case BackoffStrategy.ExponentialJitter:
                // Exponential with decorrelated jitter factor in [0.5, 1.5)
                var exp = baseMs * Math.Pow(2, attempt - 1);
                var jitter = 0.5 + _rng.NextDouble(); // 0.5..1.5
                ms = exp * jitter;
                break;
        }

        if (ms > maxMs) ms = maxMs;
        if (ms < 0) ms = 0;
        return TimeSpan.FromMilliseconds(ms);
    }

    private static bool DefaultShouldRetry(Exception ex)
    {
        // Retry on common transient categories; customize as needed in ctor
        return ex is TimeoutException
            || ex is System.Net.Http.HttpRequestException
            || ex is System.IO.IOException
            || ex.GetType().Name.Contains("Transient", StringComparison.OrdinalIgnoreCase);
    }
}
