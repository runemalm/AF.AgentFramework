namespace AgentFramework.Tools.Registry;

/// <summary>
/// Context used when resolving/listing tools, allowing per-agent/role/environment scoping
/// and simple allow/deny name filters. Providers may extend this in the future.
/// </summary>
public sealed record class ToolBindingContext
{
    /// <summary>
    /// Identifier of the calling agent (for auditing and per-agent policy overlays).
    /// </summary>
    public string AgentId { get; init; } = string.Empty;

    /// <summary>
    /// Optional role or capability profile of the agent (e.g., "reader", "operator").
    /// </summary>
    public string? Role { get; init; }

    /// <summary>
    /// Logical environment (e.g., "Dev", "Staging", "Prod") for policy selection.
    /// </summary>
    public string? Environment { get; init; }

    /// <summary>
    /// Optional allowlist of fully qualified tool names. If set, only these names are visible.
    /// </summary>
    public ISet<string>? AllowList { get; init; }

    /// <summary>
    /// Optional denylist of fully qualified tool names. Deny takes precedence over allow.
    /// </summary>
    public ISet<string>? DenyList { get; init; }
}