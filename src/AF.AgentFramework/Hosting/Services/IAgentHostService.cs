namespace AgentFramework.Hosting.Services;

public interface IAgentHostService
{
    Task StartAsync(IAgentHostContext context, CancellationToken ct);
    Task StopAsync(CancellationToken ct);
}
