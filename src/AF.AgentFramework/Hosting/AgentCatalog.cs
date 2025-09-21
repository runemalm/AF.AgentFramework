using AgentFramework.Kernel;

namespace AgentFramework.Hosting;

/// <summary>Simple in-proc catalog holding singleton agent instances.</summary>
public sealed class AgentCatalog : IAgentCatalog
{
    private readonly Dictionary<string, IAgent> _agents;

    public AgentCatalog(Dictionary<string, IAgent> agents)
        => _agents = agents ?? throw new ArgumentNullException(nameof(agents));

    public IAgent Get(string agentId)
        => _agents.TryGetValue(agentId, out var a)
            ? a
            : throw new KeyNotFoundException($"Agent '{agentId}' not registered.");

    public IReadOnlyCollection<string> ListIds() => _agents.Keys;
}
