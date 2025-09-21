namespace AgentFramework.Runners;

public interface IRunner
{
    string Name { get; }
    Task StartAsync(CancellationToken ct = default);
    Task StopAsync(CancellationToken ct = default);
}
