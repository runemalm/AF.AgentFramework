using AgentFramework.Tools.Contracts;
using AgentFramework.Tools.Registry;
using AgentFramework.Tools.Runtime;

namespace AgentFramework.Tools.Integration.Initialization;

internal sealed class ToolRegistryInitializer : IToolRegistryInitializer
{
    public void Initialize(IToolRegistry registry, IEnumerable<ILocalTool> tools)
    {
        foreach (var tool in tools)
        {
            registry.Publish(new ToolDescriptor
            {
                Name = tool.Name,
                Version = tool.Version,
                Contract = tool.Contract,
                Origin = "local"
            });
        }
    }
}
