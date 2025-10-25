using AgentFramework.Kernel.Knowledge;
using AgentFramework.Tools.Integration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

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
    public IKnowledge Knowledge { get; }
    public AgentContextTools? Tools { get; }
    private readonly ILogger<AgentContext> _logger;

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
        IKnowledge? knowledge,
        AgentContextTools? tools,
        ILogger<AgentContext>? logger = null)
    {
        AgentId = agentId;
        EngineId = engineId;
        WorkItemId = workItemId;
        CorrelationId = correlationId;
        Cancellation = cancellation;
        Random = new Random(randomSeed);
        Knowledge = knowledge ?? new InMemoryKnowledge();
        Tools = tools;
        _logger = logger ?? NullLogger<AgentContext>.Instance;
    }

    public void Trace(string message, IReadOnlyDictionary<string, object?>? data = null)
    {
        _logger.LogInformation("[Context:{AgentId}] {Message} {@Data}", AgentId, message, data);
        if (data is not null)
        {
            foreach (var kv in data)
                Console.WriteLine($"  - {kv.Key}: {kv.Value}");
        }
    }
}
