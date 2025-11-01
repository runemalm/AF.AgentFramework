namespace AgentFramework.Kernel.Routing;

/// <summary>
/// Default router that broadcasts each percept to all attached agents.
/// </summary>
public sealed class BroadcastStimulusRouter : IStimulusRouter
{
    public async Task RouteAsync(
        WorkItem percept,
        IKernel kernel,
        IReadOnlyCollection<string> attachedAgents,
        CancellationToken ct = default)
    {
        foreach (var agentId in attachedAgents)
        {
            var clone = new WorkItem
            {
                Id = Guid.NewGuid().ToString("n"),
                EngineId = percept.EngineId,
                AgentId = agentId,
                Kind = WorkItemKind.Percept,
                Payload = percept.Payload,
                Metadata = percept.Metadata
            };

            Console.WriteLine($"[Router] Routed percept to '{agentId}' (topic={percept.Metadata.GetValueOrDefault("topic", "?")}).");
            await kernel.EnqueueAsync(clone, ct).ConfigureAwait(false);
        }
    }
}
