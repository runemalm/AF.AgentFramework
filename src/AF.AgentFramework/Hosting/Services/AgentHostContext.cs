using AgentFramework.Kernel;
using AgentFramework.Engines;

namespace AgentFramework.Hosting.Services;

internal sealed class AgentHostContext : IAgentHostContext
{
    public IKernel Kernel { get; }
    public IReadOnlyDictionary<string, IEngine> Engines { get; }
    public IServiceProvider Services { get; }

    public AgentHostContext(IKernel kernel,
        IReadOnlyDictionary<string, IEngine> engines,
        IServiceProvider services)
    {
        Kernel = kernel;
        Engines = engines;
        Services = services;
    }
}
