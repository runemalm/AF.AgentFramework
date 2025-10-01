namespace AgentFramework.Tools.Contracts;

/// <summary>
/// Result envelope returned from a tool invocation.
/// </summary>
public sealed record class ToolResult
{
    /// <summary>
    /// Overall outcome classification.
    /// </summary>
    public ToolResultStatus Status { get; init; }

    /// <summary>
    /// Successful output (present when Status == Ok).
    /// </summary>
    public object? Output { get; init; }

    /// <summary>
    /// Structured error (present when Status != Ok).
    /// </summary>
    public ToolError? Error { get; init; }

    /// <summary>
    /// End-to-end duration in milliseconds, including policy overheads.
    /// </summary>
    public int DurationMs { get; init; }

    /// <summary>
    /// Total number of attempts performed (>= 1).
    /// </summary>
    public int Attempts { get; init; } = 1;

    /// <summary>
    /// Concrete version of the tool that was executed (if resolution succeeded).
    /// </summary>
    public string? ResolvedVersion { get; init; }

    /// <summary>
    /// Snapshot of the effective policies at execution time.
    /// </summary>
    public PolicySnapshot? PolicySnapshot { get; init; }

    /// <summary>
    /// Correlation identifier echoing the invocation for tracing.
    /// </summary>
    public string CorrelationId { get; init; } = string.Empty;

    /// <summary>
    /// Origin of the executed tool (e.g., "local", "mcp::<server>", or "unknown").
    /// </summary>
    public string? Origin { get; init; }

    /// <summary>
    /// Optional timestamp when the invocation completed (UTC).
    /// </summary>
    public DateTimeOffset? CompletedUtc { get; init; }
}
