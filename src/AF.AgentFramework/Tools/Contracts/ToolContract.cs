namespace AgentFramework.Tools.Contracts;

/// <summary>
/// Immutable description of a tool's interface and default behavior.
/// Hosts may overlay policies; the contract remains the source of truth for I/O and effects.
/// </summary>
public sealed record class ToolContract
{
    /// <summary>
    /// Fully qualified name (e.g., "local::fs.write", "mcp::github::issues.create").
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Concrete version string (SemVer recommended for local tools).
    /// </summary>
    public string Version { get; init; } = "1.0.0";

    /// <summary>
    /// Human-readable description of the capability and scope.
    /// </summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// Opaque schema object describing required/optional input fields.
    /// </summary>
    public object? InputSchema { get; init; }

    /// <summary>
    /// Opaque schema object describing the successful output shape.
    /// </summary>
    public object? OutputSchema { get; init; }

    /// <summary>
    /// Declared side-effect semantics for policy enforcement.
    /// </summary>
    public EffectLevel Effect { get; init; } = EffectLevel.Pure;

    /// <summary>
    /// Default reliability/safety policy hints for this tool.
    /// </summary>
    public PolicyDefaults DefaultPolicies { get; init; } = new();

    /// <summary>
    /// Arbitrary metadata for classification/ownership (e.g., "owner","sla","tags").
    /// </summary>
    public IDictionary<string, string> Metadata { get; init; } = new Dictionary<string, string>();
}
