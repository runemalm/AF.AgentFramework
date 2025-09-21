using AgentFramework.Kernel.Policies;

namespace AgentFramework.Hosting;

public sealed class Attachment
{
    public required string AgentId { get; init; }
    public required string EngineId { get; init; }
    public PolicySet? Overrides { get; init; }
}
