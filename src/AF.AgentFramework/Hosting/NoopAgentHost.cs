namespace AgentFramework.Hosting;

/// <summary>
/// Temporary no-op host so the builder compiles and tests can target the surface.
/// </summary>
public sealed class NoopAgentHost : IAgentHost
{
    public AgentHostConfig Config { get; }

    public NoopAgentHost(AgentHostConfig config) => Config = config;

    public Task StartAsync(CancellationToken ct = default) => Task.CompletedTask;
    public Task StopAsync(CancellationToken ct = default) => Task.CompletedTask;
}
