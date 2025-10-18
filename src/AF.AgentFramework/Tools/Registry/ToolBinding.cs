namespace AgentFramework.Tools.Registry;

// reserved for future: tool visibility filtering

/// <summary>
/// Simple allow/deny binding rule for tool discovery and invocation.
/// This is a placeholder for future, richer binding semantics.
/// </summary>
public sealed record class ToolBinding
{
    /// <summary>
    /// Glob-like name pattern (e.g., "local::fs.*", "mcp::github::issues.create").
    /// Matching is case-insensitive; exact match if no wildcard is present.
    /// </summary>
    public string NamePattern { get; init; } = "*";

    /// <summary>
    /// If true, the pattern allows matching tools; if false, it denies them.
    /// Deny rules should take precedence when both allow and deny match.
    /// </summary>
    public bool Allowed { get; init; } = true;
}