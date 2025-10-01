namespace AgentFramework.Tools.Contracts;

/// <summary>
/// Invocation envelope used to call a tool via the pipeline.
/// </summary>
public sealed record class ToolInvocation
{
    /// <summary>
    /// Fully qualified tool name (e.g., "local::fs.write", "mcp::github::issues.create").
    /// </summary>
    public string ToolName { get; init; } = string.Empty;

    /// <summary>
    /// Optional version or version range (e.g., "^1.2"). If null, the registry selects the preferred version.
    /// </summary>
    public string? VersionRange { get; init; }

    /// <summary>
    /// Input payload validated against the tool's input schema.
    /// </summary>
    public object? Input { get; init; }

    /// <summary>
    /// Correlation identifier for tracing across systems. Generated if not supplied.
    /// </summary>
    public string? CorrelationId { get; init; }

    /// <summary>
    /// Optional identifier of the triggering event or parent invocation.
    /// </summary>
    public string? CausationId { get; init; }

    /// <summary>
    /// Required for idempotent writes to enable safe retries.
    /// </summary>
    public string? IdempotencyKey { get; init; }

    /// <summary>
    /// Absolute deadline (UTC). The policy stage enforces this as a timeout.
    /// </summary>
    public DateTimeOffset? DeadlineUtc { get; init; }

    /// <summary>
    /// Non-sensitive headers/context (e.g., locale, tenant). Do not place secrets here.
    /// </summary>
    public IDictionary<string, string>? Headers { get; init; }

    /// <summary>
    /// Caller-supplied metadata for audit/diagnostics. May be redacted by policy.
    /// </summary>
    public IDictionary<string, string>? Metadata { get; init; }
}
