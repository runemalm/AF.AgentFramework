using AgentFramework.Kernel;
using AgentFramework.Kernel.Policies;
using AgentFramework.Engines;
using AgentFramework.Runners;

namespace AgentFramework.Hosting;

public interface IAgentHostBuilder
{
    IAgentHostBuilder AddEngine(string engineId, Func<IEngine> engineFactory);
    IAgentHostBuilder AddRunner(string engineId, Func<IRunner> runnerFactory);
    IAgentHostBuilder AddAgent(string agentId, Func<IAgent> agentFactory);
    IAgentHostBuilder Attach(string agentId, string engineId, PolicySet? overrides = null);
    IAgentHostBuilder WithKernelDefaults(PolicySet defaults);
    IAgentHost Build();
}
