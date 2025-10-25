using AgentFramework.Kernel;
using AgentFramework.Kernel.Policies;
using AgentFramework.Engines;
using AgentFramework.Runners;
using Microsoft.Extensions.DependencyInjection;

namespace AgentFramework.Hosting;

/// <summary>
/// Fluent builder interface for composing and configuring an AgentHost.
/// </summary>
public interface IAgentHostBuilder
{
    /// <summary>Access to the underlying service collection for advanced scenarios.</summary>
    IServiceCollection Services { get; }

    /// <summary>Adds an engine factory to the host.</summary>
    IAgentHostBuilder AddEngine(string engineId, Func<IEngine> engineFactory);

    /// <summary>Adds a runner factory associated with a specific engine.</summary>
    IAgentHostBuilder AddRunner(string engineId, Func<IRunner> runnerFactory);

    /// <summary>Adds an agent factory to the host.</summary>
    IAgentHostBuilder AddAgent(string agentId, Func<IAgent> agentFactory);

    /// <summary>Attaches an agent to an engine, optionally with overridden policies.</summary>
    IAgentHostBuilder Attach(string agentId, string engineId, PolicySet? overrides = null);

    /// <summary>Sets the default policy set for the kernel.</summary>
    IAgentHostBuilder WithKernelDefaults(PolicySet defaults);

    /// <summary>Sets the kernel worker concurrency level.</summary>
    AgentHostBuilder WithKernelConcurrency(int workerCount);

    /// <summary>Specifies the kernel factory implementation to use.</summary>
    IAgentHostBuilder WithKernel(Func<IKernelFactory> factory);

    /// <summary>Enables the live observability dashboard on a given port.</summary>
    IAgentHostBuilder EnableDashboard(int port = 6060);

    /// <summary>Builds and returns the configured AgentHost.</summary>
    IAgentHost Build();
}
