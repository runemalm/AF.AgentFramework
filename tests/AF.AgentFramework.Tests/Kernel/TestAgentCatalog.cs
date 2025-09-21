using AgentFramework.Kernel;

namespace AgentFramework.Tests.Kernel;

internal sealed class TestAgentCatalog : IAgentCatalog
{
    private readonly Dictionary<string, IAgent> _map = new(StringComparer.Ordinal);
    public TestAgentCatalog(params IAgent[] agents)
    {
        foreach (var a in agents) _map[a.Id] = a;
    }

    public IAgent Get(string agentId)
        => _map.TryGetValue(agentId, out var a)
            ? a
            : throw new KeyNotFoundException(agentId);

    public IReadOnlyCollection<string> ListIds() => _map.Keys;
}
