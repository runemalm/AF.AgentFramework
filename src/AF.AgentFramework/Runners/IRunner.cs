namespace AgentFramework.Runners;

public interface IRunner
{
    string Name { get; }
    string EngineId { get; set; }
    Task StartAsync(CancellationToken ct = default);
    Task StopAsync(CancellationToken ct = default);
}
