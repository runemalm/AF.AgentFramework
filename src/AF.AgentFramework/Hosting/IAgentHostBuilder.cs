using AgentFramework.Kernel;
using AgentFramework.Kernel.Policies;
using AgentFramework.Engines;
using AgentFramework.Hosting.Services;
using AgentFramework.Runners;
using Microsoft.Extensions.DependencyInjection;

namespace AgentFramework.Hosting;

public interface IAgentHostBuilder
{
    IServiceCollection Services { get; }
    
    IAgentHostBuilder AddEngine(string engineId, Func<IEngine> engineFactory);
    IAgentHostBuilder AddRunner(string engineId, Func<IRunner> runnerFactory);
    IAgentHostBuilder AddAgent(string agentId, Func<IAgent> agentFactory);
    IAgentHostBuilder Attach(string agentId, string engineId, PolicySet? overrides = null);
    IAgentHostBuilder WithKernelDefaults(PolicySet defaults);
    IAgentHostBuilder WithKernel(Func<IKernelFactory> factory);
    IAgentHostBuilder AddHostService(Func<IAgentHostService> factory);
    IAgentHostBuilder EnableDashboard(int port = 6060);
    IAgentHost Build();
}
