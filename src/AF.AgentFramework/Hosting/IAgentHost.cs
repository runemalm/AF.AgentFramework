namespace AgentFramework.Hosting;

public interface IAgentHost
{
    Task StartAsync(CancellationToken ct = default);
    Task StopAsync(CancellationToken ct = default);
}
