namespace AgentFramework.Kernel;

public interface IPolicy
{
    Task<Decision> DecideAsync(AgentContext context, CancellationToken ct = default);
}

public interface ITool
{
    string Name { get; }
    string Description { get; }
    Task<ToolResult> InvokeAsync(ToolContext context, string argumentsJson, CancellationToken ct = default);
}

public interface IToolRegistry
{
    ITool? Get(string name);
    IEnumerable<ITool> All();
}

public interface IMemoryStore
{
    Task AppendAsync(MemoryEntry entry, CancellationToken ct = default);
    IAsyncEnumerable<MemoryEntry> QueryAsync(MemoryQuery query, CancellationToken ct = default);
}

public record MemoryEntry(DateTimeOffset At, string Kind, string Json);
public record MemoryQuery(string Kind, string FilterJson);

public interface IMetadataBag
{
    T? Get<T>(string key);
    void Set<T>(string key, T value);
}

public sealed class ToolContext
{
    public AgentContext Agent { get; }
    public IServiceProvider? Services { get; }
    public ToolContext(AgentContext agent, IServiceProvider? services = null)
    { Agent = agent; Services = services; }
}
