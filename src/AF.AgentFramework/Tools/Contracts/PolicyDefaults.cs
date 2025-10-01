namespace AgentFramework.Tools.Contracts;

/// <summary>
/// Default policy hints declared by a tool's contract. Hosts may overlay or tighten these.
/// </summary>
public sealed record class PolicyDefaults
{
    /// <summary>
    /// Execution deadline per attempt, in milliseconds. If null, the host default applies.
    /// </summary>
    public int? TimeoutMs { get; init; }

    /// <summary>
    /// Suggested retry configuration for idempotent tools. This is advisory; the host decides.
    /// Not used in the initial slice (retries disabled), but modeled for forward compatibility.
    /// </summary>
    public RetryDefaults? Retry { get; init; }

    /// <summary>
    /// Logical rate limiter identifier (e.g., "github-api"). Host may map to actual limiters.
    /// </summary>
    public string? RateLimitId { get; init; }

    /// <summary>
    /// Maximum in-flight concurrency suggested for this tool.
    /// </summary>
    public int? ConcurrencyLimit { get; init; }

    /// <summary>
    /// Logical budget identifier for cost/time governance (e.g., "external.calls").
    /// </summary>
    public string? BudgetId { get; init; }
}

/// <summary>
/// Advisory retry settings. Pipelines must only honor these for idempotent tools,
/// and hosts may override or disable them.
/// </summary>
public sealed record class RetryDefaults
{
    /// <summary>
    /// Maximum retry attempts (not counting the first try).
    /// </summary>
    public int MaxAttempts { get; init; } = 0;

    /// <summary>
    /// Base delay in milliseconds for backoff.
    /// </summary>
    public int BackoffBaseMs { get; init; } = 200;

    /// <summary>
    /// Backoff multiplier (e.g., 2 for exponential).
    /// </summary>
    public double BackoffMultiplier { get; init; } = 2.0;

    /// <summary>
    /// Jitter factor in [0,1], where 0.2 means ±20% randomization.
    /// </summary>
    public double JitterFactor { get; init; } = 0.2;
}
