using System.Collections.Concurrent;
using AgentFramework.Tools.Contracts;

namespace AgentFramework.Tools.Registry;

/// <summary>
/// Simple in-memory implementation of <see cref="IToolRegistry"/>.
/// Stores descriptors in dictionaries keyed by name and version.
/// Not thread-safe for fine-grained ops, but safe for typical publish/resolve/list patterns.
/// </summary>
public sealed class InMemoryToolRegistry : IToolRegistry
{
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, ToolDescriptor>> _store = new();

    public void Publish(ToolDescriptor descriptor)
    {
        var versions = _store.GetOrAdd(descriptor.Name, _ => new ConcurrentDictionary<string, ToolDescriptor>(StringComparer.OrdinalIgnoreCase));
        versions[descriptor.Version] = descriptor;
    }

    public bool Unpublish(string name, string version)
    {
        if (_store.TryGetValue(name, out var versions))
        {
            return versions.TryRemove(version, out _);
        }
        return false;
    }

    public ToolDescriptor? Resolve(string name, string? versionRange, ToolBindingContext bindingContext)
    {
        if (!_store.TryGetValue(name, out var versions) || versions.Count == 0)
            return null;

        var visible = ApplyBindings(versions.Values, bindingContext).ToList();
        if (visible.Count == 0)
            return null;

        var selectedVersion = VersionSelector.Select(visible.Select(v => v.Version).ToList(), versionRange);
        return visible.FirstOrDefault(v => v.Version.Equals(selectedVersion, StringComparison.OrdinalIgnoreCase));
    }

    public IReadOnlyList<ToolDescriptor> List(ToolBindingContext bindingContext)
    {
        var all = _store.Values.SelectMany(v => v.Values);
        return ApplyBindings(all, bindingContext).ToList();
    }

    private static IEnumerable<ToolDescriptor> ApplyBindings(IEnumerable<ToolDescriptor> descriptors, ToolBindingContext ctx)
    {
        var filtered = descriptors;

        if (ctx.AllowList is { Count: > 0 })
            filtered = filtered.Where(d => ctx.AllowList.Contains(d.Name));

        if (ctx.DenyList is { Count: > 0 })
            filtered = filtered.Where(d => !ctx.DenyList.Contains(d.Name));

        return filtered;
    }
}
