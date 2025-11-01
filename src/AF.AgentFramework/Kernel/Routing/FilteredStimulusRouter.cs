namespace AgentFramework.Kernel.Routing;

/// <summary>
/// Routes percepts to agents based on declarative or registered topic filters.
/// Supports MQTT-style wildcards (*, /*, /#) for flexible matching.
/// </summary>
public sealed class FilteredStimulusRouter : IStimulusRouter
{
    private readonly IPerceptFilterRegistry _filters;

    public FilteredStimulusRouter(IPerceptFilterRegistry filters)
    {
        _filters = filters ?? throw new ArgumentNullException(nameof(filters));
    }

    public async Task RouteAsync(
        WorkItem percept,
        IKernel kernel,
        IReadOnlyCollection<string> attachedAgents,
        CancellationToken ct = default)
    {
        if (!percept.Metadata.TryGetValue("topic", out var topic) || string.IsNullOrWhiteSpace(topic))
            topic = "unknown";

        topic = topic.ToLowerInvariant();

        foreach (var agentId in attachedAgents)
        {
            var patterns = _filters.GetPatterns(agentId);
            if (patterns is null || patterns.Count == 0)
                continue;

            foreach (var pattern in patterns)
            {
                if (TopicPatternMatcher.Matches(topic, pattern))
                {
                    var clone = new WorkItem
                    {
                        Id = Guid.NewGuid().ToString("n"),
                        EngineId = percept.EngineId,
                        AgentId = agentId,
                        Kind = percept.Kind,
                        Payload = percept.Payload,
                        Metadata = percept.Metadata
                    };

                    await kernel.EnqueueAsync(clone, ct).ConfigureAwait(false);
                    break; // avoid double delivery for same agent
                }
            }
        }
    }
}
