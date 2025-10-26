using System.Collections.Concurrent;
using AgentFramework.Kernel;
using AgentFramework.Kernel.Features;

namespace AgentFramework.Mas.Features;

/// <summary>
/// Directory Feature – provides a shared registry of all active agents within the host.
/// Theory-aligned with the MAS concept of a "Directory Facilitator".
/// </summary>
public interface IMasDirectory : IAgentFeature
{
    void Register(string agentId, IEnumerable<string>? roles = null, IEnumerable<string>? capabilities = null);
    void Unregister(string agentId);

    MasAgentInfo? FindById(string agentId);
    IReadOnlyList<MasAgentInfo> FindByRole(string role);
    IReadOnlyList<MasAgentInfo> FindByCapability(string capability);
    IReadOnlyList<MasAgentInfo> ListAll();
}

/// <summary>
/// Simple record representing an agent registered in the MAS Directory.
/// </summary>
public record MasAgentInfo(
    string AgentId,
    IReadOnlyList<string> Roles,
    IReadOnlyList<string> Capabilities
);

/// <summary>
/// Default in-memory implementation of IMasDirectory.
/// Shared across all agents in a single host.
/// </summary>
public sealed class MasDirectoryFeature : IMasDirectory
{
    private readonly ConcurrentDictionary<string, MasAgentInfo> _agents =
        new(StringComparer.OrdinalIgnoreCase);

    public void Register(string agentId, IEnumerable<string>? roles = null, IEnumerable<string>? capabilities = null)
    {
        if (string.IsNullOrWhiteSpace(agentId))
            throw new ArgumentException("AgentId is required.", nameof(agentId));

        var info = new MasAgentInfo(
            agentId,
            (roles ?? Array.Empty<string>()).ToList(),
            (capabilities ?? Array.Empty<string>()).ToList()
        );

        _agents[agentId] = info;
    }

    public void Unregister(string agentId)
    {
        _agents.TryRemove(agentId, out _);
    }

    public MasAgentInfo? FindById(string agentId)
        => _agents.TryGetValue(agentId, out var info) ? info : null;

    public IReadOnlyList<MasAgentInfo> FindByRole(string role)
        => _agents.Values
            .Where(a => a.Roles.Contains(role, StringComparer.OrdinalIgnoreCase))
            .ToList();

    public IReadOnlyList<MasAgentInfo> FindByCapability(string capability)
        => _agents.Values
            .Where(a => a.Capabilities.Contains(capability, StringComparer.OrdinalIgnoreCase))
            .ToList();

    public IReadOnlyList<MasAgentInfo> ListAll()
        => _agents.Values.ToList();
    
    public void Attach(IAgentContext context)
    {
        Register(context.AgentId);
    }
}
