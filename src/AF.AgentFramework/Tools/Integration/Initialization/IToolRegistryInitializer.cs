using AgentFramework.Tools.Registry;
using AgentFramework.Tools.Runtime;

namespace AgentFramework.Tools.Integration.Initialization;

internal interface IToolRegistryInitializer
{
    void Initialize(IToolRegistry registry, IEnumerable<ILocalTool> tools);
}
