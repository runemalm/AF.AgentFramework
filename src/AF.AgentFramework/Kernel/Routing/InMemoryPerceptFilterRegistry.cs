using System.Collections.Concurrent;

namespace AgentFramework.Kernel.Routing;

/// <summary>
/// Default in-memory implementation of <see cref="IPerceptFilterRegistry"/>.
/// Stores per-agent filter patterns (topics) and predicate delegates for fine-grained matching.
/// </summary>
public sealed class InMemoryPerceptFilterRegistry : IPerceptFilterRegistry
{
    private readonly ConcurrentDictionary<string, AgentFilterSet> _filters =
        new(StringComparer.Ordinal);

    private sealed class AgentFilterSet
    {
        public List<string> Patterns { get; } = new();
        public List<Func<object?, bool>> Predicates { get; } = new();
    }

    /// <inheritdoc />
    public void Register(string agentId, string filterKey)
    {
        if (string.IsNullOrWhiteSpace(agentId))
            throw new ArgumentException("AgentId is required.", nameof(agentId));
        if (string.IsNullOrWhiteSpace(filterKey))
            throw new ArgumentException("Filter key is required.", nameof(filterKey));

        var set = _filters.GetOrAdd(agentId, _ => new AgentFilterSet());
        lock (set)
        {
            if (!set.Patterns.Contains(filterKey, StringComparer.OrdinalIgnoreCase))
                set.Patterns.Add(filterKey);
        }
    }

    /// <inheritdoc />
    public void Register(string agentId, Func<object?, bool> predicate)
    {
        if (string.IsNullOrWhiteSpace(agentId))
            throw new ArgumentException("AgentId is required.", nameof(agentId));
        if (predicate is null)
            throw new ArgumentNullException(nameof(predicate));

        var set = _filters.GetOrAdd(agentId, _ => new AgentFilterSet());
        lock (set)
        {
            set.Predicates.Add(predicate);
        }
    }

    /// <inheritdoc />
    public bool IsMatch(string agentId, object? payload, string? topic)
    {
        if (!_filters.TryGetValue(agentId, out var set))
            return false;

        lock (set)
        {
            // 1. Match against declarative string patterns (topics)
            if (topic is not null)
            {
                foreach (var p in set.Patterns)
                {
                    if (TopicPatternMatcher.Matches(topic, p))
                        return true;
                }
            }

            // 2. Match against runtime predicates (payload-based)
            foreach (var pred in set.Predicates)
            {
                try
                {
                    if (pred(payload))
                        return true;
                }
                catch
                {
                    // ignore predicate exceptions
                }
            }

            return false;
        }
    }

    /// <inheritdoc />
    public IReadOnlyList<string> GetPatterns(string agentId)
    {
        if (!_filters.TryGetValue(agentId, out var set))
            return Array.Empty<string>();

        lock (set)
        {
            return set.Patterns.ToList();
        }
    }
}
