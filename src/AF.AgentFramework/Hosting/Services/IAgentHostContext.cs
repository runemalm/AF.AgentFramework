using AgentFramework.Kernel;
using AgentFramework.Engines;

namespace AgentFramework.Hosting.Services;

public interface IAgentHostContext
{
    IKernel Kernel { get; }
    IReadOnlyDictionary<string, IEngine> Engines { get; }
    IServiceProvider Services { get; }
}
