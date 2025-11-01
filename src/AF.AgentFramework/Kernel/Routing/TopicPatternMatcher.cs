namespace AgentFramework.Kernel.Routing;

/// <summary>
/// Provides standardized topic-matching semantics for percept routing.
/// Inspired by MAS blackboard subscriptions and MQTT-style topic filters.
/// </summary>
public static class TopicPatternMatcher
{
    /// <summary>
    /// Determines whether a given <paramref name="topic"/> matches a filter <paramref name="pattern"/>.
    /// Supports the following forms:
    /// <list type="bullet">
    /// <item><description><c>*</c> — matches everything</description></item>
    /// <item><description><c>prefix/*</c> — matches one level under the prefix</description></item>
    /// <item><description><c>prefix/#</c> — matches recursively under the prefix</description></item>
    /// <item><description>otherwise — exact match</description></item>
    /// </list>
    /// Matching is case-insensitive.
    /// </summary>
    public static bool Matches(string topic, string pattern)
    {
        if (string.IsNullOrWhiteSpace(topic) || string.IsNullOrWhiteSpace(pattern))
            return false;

        topic = topic.ToLowerInvariant();
        pattern = pattern.ToLowerInvariant();

        if (pattern == "*")
            return true;

        // Recursive multi-level match (e.g. slack/#)
        if (pattern.EndsWith("/#", StringComparison.Ordinal))
        {
            var prefix = pattern[..^2];
            return topic.StartsWith(prefix, StringComparison.OrdinalIgnoreCase);
        }

        // Single-level match (e.g. slack/*)
        if (pattern.EndsWith("/*", StringComparison.Ordinal))
        {
            var prefix = pattern[..^2];
            if (!topic.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                return false;

            var remaining = topic.Length > prefix.Length
                ? topic[prefix.Length..].TrimStart('/')
                : string.Empty;

            return !remaining.Contains('/');
        }

        // Exact match
        return string.Equals(topic, pattern, StringComparison.OrdinalIgnoreCase);
    }
}
