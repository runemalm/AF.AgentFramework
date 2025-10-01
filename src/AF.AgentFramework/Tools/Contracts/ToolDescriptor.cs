namespace AgentFramework.Tools.Contracts;

/// <summary>
/// A published tool instance available for resolution and invocation.
/// Couples identity (name, version) with its contract and origin.
/// </summary>
public sealed record class ToolDescriptor
{
    /// <summary>
    /// Fully qualified tool name (e.g., "local::fs.write", "mcp::github::issues.create").
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Concrete version string of this published tool.
    /// </summary>
    public string Version { get; init; } = "1.0.0";

    /// <summary>
    /// Declarative contract describing I/O, effects, and default policies.
    /// </summary>
    public ToolContract Contract { get; init; } = new();

    /// <summary>
    /// Origin of the implementation (e.g., "local", "mcp::<server>", or "unknown").
    /// </summary>
    public string Origin { get; init; } = "unknown";

    /// <summary>
    /// Optional classification tags (e.g., "filesystem", "http", "idempotent").
    /// </summary>
    public IReadOnlyCollection<string> Tags { get; init; } = new List<string>();
}
