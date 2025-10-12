using AgentFramework.Kernel.Policies;

namespace AgentFramework.Kernel;

/// <summary>Options for constructing a Kernel instance.</summary>
public sealed class KernelOptions
{
    public required IAgentCatalog Agents { get; init; }
    public required PolicySet Defaults { get; init; }

    /// <summary>Effective per-attachment policies (AgentId+EngineId). Optional.</summary>
    public IReadOnlyList<AttachmentBinding> Bindings { get; init; } = new List<AttachmentBinding>();
    
    /// <summary>
    /// Number of concurrent worker tasks executing agents.
    /// Defaults to half the processor count (minimum 1).
    /// </summary>
    public int WorkerCount { get; init; } = Math.Max(1, Environment.ProcessorCount / 2);
}

/// <summary>Resolved policies for a specific AgentId+EngineId attachment.</summary>
public sealed class AttachmentBinding
{
    public required string AgentId { get; init; }
    public required string EngineId { get; init; }
    public required PolicySet Policies { get; init; }
}