using AgentFramework.Kernel.Knowledge;

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

    /// <summary>
    /// Agent Knowledge store (the K in MAPE-K). Defaults to <see cref="InMemoryKnowledge"/> if not provided.
    /// </summary>
    public IKnowledge Knowledge { get; }

    /// <summary>
    /// Original constructor preserved for compatibility. Uses an in-memory Knowledge store.
    /// </summary>
    public AgentContext(
        string agentId,
        string engineId,
        string workItemId,
        string? correlationId,
        CancellationToken cancellation,
        int randomSeed)
        : this(agentId, engineId, workItemId, correlationId, cancellation, randomSeed, knowledge: null)
    { }

    /// <summary>
    /// Overload that allows passing a custom Knowledge store.
    /// </summary>
    public AgentContext(
        string agentId,
        string engineId,
        string workItemId,
        string? correlationId,
        CancellationToken cancellation,
        int randomSeed,
        IKnowledge? knowledge)
    {
        AgentId = agentId;
        EngineId = engineId;
        WorkItemId = workItemId;
        CorrelationId = correlationId;
        Cancellation = cancellation;
        Random = new Random(randomSeed);
        Knowledge = knowledge ?? new InMemoryKnowledge();
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
