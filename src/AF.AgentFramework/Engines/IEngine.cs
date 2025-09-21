using AgentFramework.Kernel;
using AgentFramework.Runners;

namespace AgentFramework.Engines;

public interface IEngine
{
    string Id { get; }

    /// <summary>Kernel will be provided by the Host before StartAsync.</summary>
    void BindKernel(IKernel kernel);

    /// <summary>Host registers runners on the engine prior to StartAsync.</summary>
    void AddRunner(IRunner runner);

    /// <summary>Host provides the list of agent ids attached to this engine.</summary>
    void SetAttachments(IReadOnlyList<string> agentIds);

    Task StartAsync(CancellationToken ct = default);
    Task StopAsync(CancellationToken ct = default);
}
