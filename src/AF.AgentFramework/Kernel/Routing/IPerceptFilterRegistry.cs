namespace AgentFramework.Kernel.Routing;

public interface IPerceptFilterRegistry
{
    void Register(string agentId, string filterKey);
    void Register(string agentId, Func<object?, bool> predicate);
    bool IsMatch(string agentId, object? payload, string? topic);

    /// <summary>
    /// Returns all registered filter patterns for a specific agent.
    /// Used by the router for routing decisions.
    /// </summary>
    IReadOnlyList<string> GetPatterns(string agentId);
}