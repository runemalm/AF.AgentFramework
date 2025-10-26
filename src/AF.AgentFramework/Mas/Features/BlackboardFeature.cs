using System.Collections.Concurrent;
using AgentFramework.Kernel;
using AgentFramework.Kernel.Features;

namespace AgentFramework.Mas.Features;

/// <summary>
/// Blackboard Feature — provides a shared, thread-safe memory surface for inter-agent coordination.
/// Theory-aligned with the MAS concept of a "Blackboard Environment".
/// </summary>
public interface IMasBlackboard : IAgentFeature
{
    /// <summary>Post or update a fact on the shared blackboard.</summary>
    void Post(string key, object value);

    /// <summary>Try to read a fact from the blackboard.</summary>
    bool TryRead<T>(string key, out T? value);

    /// <summary>Remove a fact from the blackboard.</summary>
    bool Remove(string key);

    /// <summary>Enumerate all current facts as key/value pairs.</summary>
    IReadOnlyDictionary<string, object> Enumerate();
}

/// <summary>
/// Default in-memory implementation of IMasBlackboard.
/// Shared between all agents in the same host.
/// </summary>
public sealed class MasBlackboardFeature : IMasBlackboard
{
    private readonly ConcurrentDictionary<string, object> _facts = new(StringComparer.OrdinalIgnoreCase);

    public void Post(string key, object value)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Key is required.", nameof(key));
        _facts[key] = value;
    }

    public bool TryRead<T>(string key, out T? value)
    {
        if (_facts.TryGetValue(key, out var obj) && obj is T cast)
        {
            value = cast;
            return true;
        }

        value = default;
        return false;
    }

    public bool Remove(string key) => _facts.TryRemove(key, out _);

    public IReadOnlyDictionary<string, object> Enumerate() => new Dictionary<string, object>(_facts);

    /// <summary>
    /// Called when the feature is attached to an agent context.
    /// Currently a no-op, but kept for symmetry and possible future use (e.g., per-agent view tracking).
    /// </summary>
    public void Attach(IAgentContext context)
    {
        // No per-agent initialization required for now
    }
}
