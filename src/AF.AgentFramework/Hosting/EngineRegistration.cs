using AgentFramework.Engines;

namespace AgentFramework.Hosting;

public sealed class EngineRegistration
{
    public required string EngineId { get; init; }
    public required Func<IEngine> Factory { get; init; }
}
