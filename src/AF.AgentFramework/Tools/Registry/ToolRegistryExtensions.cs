using AgentFramework.Tools.Contracts;

namespace AgentFramework.Tools.Registry;

/// <summary>
/// Convenience helpers for working with <see cref="IToolRegistry"/>.
/// </summary>
public static class ToolRegistryExtensions
{
    /// <summary>
    /// Publish a batch of tool descriptors.
    /// </summary>
    public static void PublishRange(this IToolRegistry registry, IEnumerable<ToolDescriptor> descriptors)
    {
        foreach (var d in descriptors)
            registry.Publish(d);
    }

    /// <summary>
    /// Try resolve and return a boolean indicating success.
    /// </summary>
    public static bool TryResolve(
        this IToolRegistry registry,
        string name,
        string? versionRange,
        ToolBindingContext bindingContext,
        out ToolDescriptor? descriptor)
    {
        descriptor = registry.Resolve(name, versionRange, bindingContext);
        return descriptor is not null;
    }
}