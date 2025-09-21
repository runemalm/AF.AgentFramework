namespace AgentFramework.Kernel;

internal sealed class AgentContext : IAgentContext
{
    public string AgentId { get; }
    public string EngineId { get; }
    public string WorkItemId { get; }
    public string? CorrelationId { get; }
    public CancellationToken Cancellation { get; }
    public DateTimeOffset Now => DateTimeOffset.UtcNow;
    public Random Random { get; }
    public IDictionary<string, object?> Items { get; } = new Dictionary<string, object?>();

    public AgentContext(string agentId, string engineId, string workItemId, string? correlationId, CancellationToken cancellation, int randomSeed)
    {
        AgentId = agentId;
        EngineId = engineId;
        WorkItemId = workItemId;
        CorrelationId = correlationId;
        Cancellation = cancellation;
        Random = new Random(randomSeed);
    }

    public void Trace(string message, IReadOnlyDictionary<string, object?>? data = null)
    {
        Console.WriteLine($"[Context:{AgentId}] {message}");
        if (data is not null)
        {
            foreach (var kv in data)
                Console.WriteLine($"  - {kv.Key}: {kv.Value}");
        }
    }
}
