using AgentFramework.Kernel.Abstractions;

namespace AgentFramework.Kernel.Profiles;

/// <summary>
/// Describes a single step in a kernel tick.
/// </summary>
public sealed class LoopStepDescriptor
{
    public LoopStepId Id { get; }
    public KernelNext Terminal { get; }

    public LoopStepDescriptor(LoopStepId id, KernelNext terminal)
    {
        Id = id;
        Terminal = terminal;
    }
}
