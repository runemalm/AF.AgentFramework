using AgentFramework.Tools.Contracts;
using AgentFramework.Tools.Integration;
using AgentFramework.Tools.Registry;
using Xunit;

namespace AgentFramework.Tests.Tools;

public class AgentContextToolsTests
{
    private static ToolDescriptor Make(string name = "demo", string version = "1.0.0")
        => new()
        {
            Name = name,
            Version = version,
            Origin = "unknown",
            Contract = new ToolContract
            {
                Name = name,
                Version = version,
                Description = "test",
                Effect = EffectLevel.Pure,
                DefaultPolicies = new PolicyDefaults { TimeoutMs = 200 }
            },
            Tags = new List<string>()
        };

    [Fact]
    public void ListTools_Returns_Published_Descriptors()
    {
        var (registry, invoker) = ToolsBootstrap.CreateDefaults();
        var ctxTools = new AgentContextTools(registry, invoker, new ToolBindingContext { AgentId = "agent-1" });

        registry.Publish(Make("local::time.now", "1.0.0"));
        registry.Publish(Make("local::time.now", "1.1.0"));

        var list = ctxTools.ListTools();
        Assert.Contains(list, d => d.Name == "local::time.now" && d.Version == "1.0.0");
        Assert.Contains(list, d => d.Name == "local::time.now" && d.Version == "1.1.0");
    }

    [Fact]
    public async Task Invoke_Returns_NotImplemented_By_Default()
    {
        var (registry, invoker) = ToolsBootstrap.CreateDefaults();
        var ctxTools = new AgentContextTools(registry, invoker, new ToolBindingContext { AgentId = "agent-1" });

        var d = Make("demo", "1.0.0");
        registry.Publish(d);

        var result = await ctxTools.InvokeAsync(new ToolInvocation { ToolName = "demo" });
        Assert.NotNull(result);
        Assert.NotNull(result.Error);
        Assert.Equal(ToolErrorCode.ExecutionError, result.Error!.Code);
        Assert.Equal("NotImplemented", result.Error!.Subcode);
    }
}
