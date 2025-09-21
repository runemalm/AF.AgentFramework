using AgentFramework.Runners;

namespace AgentFramework.Hosting;

public sealed class RunnerRegistration
{
    public required string EngineId { get; init; }
    public required Func<IRunner> Factory { get; init; }
}
