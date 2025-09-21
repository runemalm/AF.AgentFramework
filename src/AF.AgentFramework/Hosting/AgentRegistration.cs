using AgentFramework.Kernel;

namespace AgentFramework.Hosting;

public sealed class AgentRegistration
{
    public required string AgentId { get; init; }
    public required Func<IAgent> Factory { get; init; }
}
