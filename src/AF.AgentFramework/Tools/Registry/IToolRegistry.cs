using AgentFramework.Tools.Contracts;

namespace AgentFramework.Tools.Registry;

/// <summary>
/// Registry of published tools. Supports publishing/unpublishing, resolution by name/version,
/// and listing tools subject to binding context.
/// </summary>
public interface IToolRegistry
{
    /// <summary>
    /// Publish a new tool descriptor into the registry.
    /// </summary>
    void Publish(ToolDescriptor descriptor);

    /// <summary>
    /// Remove a tool from the registry by name and version.
    /// Returns true if removed, false if not found.
    /// </summary>
    bool Unpublish(string name, string version);

    /// <summary>
    /// Resolve a tool descriptor by name and optional version range.
    /// Returns null if no match is found.
    /// </summary>
    ToolDescriptor? Resolve(string name, string? versionRange, ToolBindingContext bindingContext);

    /// <summary>
    /// List all tools visible under the given binding context.
    /// </summary>
    IReadOnlyList<ToolDescriptor> List(ToolBindingContext bindingContext);
}
