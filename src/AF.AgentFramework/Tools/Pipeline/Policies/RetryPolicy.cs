namespace AgentFramework.Tools.Pipeline.Policies;

/// <summary>
/// Placeholder for retry policy logic.
/// Not active in the initial slice; will be implemented when idempotent tools are supported.
/// </summary>
public sealed class RetryPolicy
{
    public int MaxAttempts { get; init; } = 0;
    public int BackoffBaseMs { get; init; } = 200;
    public double BackoffMultiplier { get; init; } = 2.0;
    public double JitterFactor { get; init; } = 0.2;

    /// <summary>
    /// Computes delay before the next attempt given the current attempt index (starting at 1).
    /// </summary>
    public TimeSpan ComputeDelay(int attempt, Random? rng = null)
    {
        if (attempt <= 0) return TimeSpan.Zero;
        var exp = BackoffBaseMs * Math.Pow(BackoffMultiplier, attempt - 1);
        var delayMs = exp;

        if (JitterFactor > 0)
        {
            rng ??= new Random();
            var jitter = 1 + (rng.NextDouble() * 2 - 1) * JitterFactor;
            delayMs *= jitter;
        }

        return TimeSpan.FromMilliseconds(Math.Max(0, delayMs));
    }
}