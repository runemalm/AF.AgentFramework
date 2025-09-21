namespace AgentFramework.Kernel;

/// <summary>Resolves agents by id for the Kernel.</summary>
public interface IAgentCatalog
{
    IAgent Get(string agentId);
    IReadOnlyCollection<string> ListIds();
}
