namespace AgentFramework.Tools.Contracts;

/// <summary>
/// Effective policy values applied to a single tool invocation at execution time.
/// </summary>
public sealed record class PolicySnapshot
{
    /// <summary>
    /// Execution deadline per attempt, in milliseconds (if enforced).
    /// </summary>
    public int? TimeoutMs { get; init; }

    /// <summary>
    /// Whether retries are enabled for this invocation (false in the initial slice).
    /// </summary>
    public bool RetryEnabled { get; init; }

    /// <summary>
    /// Identifier of the rate limiter applied (if any).
    /// </summary>
    public string? RateLimitId { get; init; }

    /// <summary>
    /// Circuit state at the time of execution (e.g., "closed", "open", "half-open").
    /// </summary>
    public string? CircuitState { get; init; }

    /// <summary>
    /// Maximum concurrent invocations allowed for this tool/provider (if enforced).
    /// </summary>
    public int? ConcurrencyLimit { get; init; }

    /// <summary>
    /// Identifier of the cost/time budget that governed this invocation (if any).
    /// </summary>
    public string? BudgetId { get; init; }
}
