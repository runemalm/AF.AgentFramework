using AgentFramework.Engines;

namespace AgentFramework.Hosting;

public sealed class EngineRegistration
{
    public required string EngineId { get; init; } = default!;
    public required Func<IServiceProvider, IEngine> Factory { get; init; } = default!;
}
