namespace AgentFramework.Tools.Contracts;

/// <summary>
/// Structured error returned from a tool invocation.
/// Use <see cref="ToolErrorCode"/> for the high-level category and <see cref="Subcode"/> for a precise reason.
/// </summary>
public sealed record class ToolError
{
    /// <summary>
    /// High-level error category.
    /// </summary>
    public ToolErrorCode Code { get; init; }

    /// <summary>
    /// Specific reason within the category (e.g., "ToolNotFound", "Timeout", "NotImplemented").
    /// </summary>
    public string? Subcode { get; init; }

    /// <summary>
    /// Human-readable, sanitized message suitable for logs and UIs.
    /// </summary>
    public string Message { get; init; } = string.Empty;

    /// <summary>
    /// Optional machine-readable payload with additional details (schema field, retryAfterMs, etc.).
    /// </summary>
    public object? Details { get; init; }

    /// <summary>
    /// Whether the failure is safe to retry according to policy and effect semantics.
    /// </summary>
    public bool IsRetryable { get; init; }

    /// <summary>
    /// Optional inner/cause error for chained failures.
    /// </summary>
    public ToolError? Cause { get; init; }

    /// <summary>
    /// Origin of the failure (e.g., "local", "mcp::server", or "unknown").
    /// </summary>
    public string? Origin { get; init; }
}
