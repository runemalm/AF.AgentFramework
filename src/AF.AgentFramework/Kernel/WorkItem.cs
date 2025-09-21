namespace AgentFramework.Kernel;

public enum WorkItemKind { Percept, Tick, Command, Job }

public sealed class WorkItem
{
    public string Id { get; init; } = default!;
    public string EngineId { get; init; } = default!;
    public string AgentId { get; init; } = default!;

    public WorkItemKind Kind { get; init; }
    public object? Payload { get; init; }

    /// <summary>Higher runs earlier. Default = 0.</summary>
    public int Priority { get; init; } = 0;

    public DateTimeOffset? Deadline { get; init; }
    public string? CorrelationId { get; init; }

    public IReadOnlyDictionary<string, string> Metadata { get; init; }
        = new Dictionary<string, string>();
}
