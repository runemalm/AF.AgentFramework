using AgentFramework.Tools.Contracts;
using AgentFramework.Tools.Registry;
using Xunit;

namespace AgentFramework.Tests.Tools;

public class InMemoryToolRegistryTests
{
    private static ToolDescriptor Make(string name, string version, string origin = "unknown")
        => new()
        {
            Name = name,
            Version = version,
            Origin = origin,
            Contract = new ToolContract
            {
                Name = name,
                Version = version,
                Description = "test",
                Effect = EffectLevel.Pure,
                DefaultPolicies = new PolicyDefaults { TimeoutMs = 500 }
            },
            Tags = new List<string>()
        };

    [Fact]
    public void Publish_Resolve_Succeeds()
    {
        var reg = new InMemoryToolRegistry();
        var ctx = new ToolBindingContext { AgentId = "test" };

        reg.Publish(Make("local::time.now", "1.0.0"));

        var d = reg.Resolve("local::time.now", null, ctx);
        Assert.NotNull(d);
        Assert.Equal("1.0.0", d!.Version);
    }

    [Fact]
    public void Resolve_NotFound_ReturnsNull()
    {
        var reg = new InMemoryToolRegistry();
        var ctx = new ToolBindingContext { AgentId = "test" };
        var d = reg.Resolve("missing", null, ctx);
        Assert.Null(d);
    }

    [Fact]
    public void List_RespectsAllowAndDeny()
    {
        var reg = new InMemoryToolRegistry();
        reg.Publish(Make("a", "1.0.0"));
        reg.Publish(Make("b", "1.0.0"));
        reg.Publish(Make("c", "1.0.0"));

        var ctx = new ToolBindingContext
        {
            AgentId = "test",
            AllowList = new HashSet<string> { "a", "b" },
            DenyList = new HashSet<string> { "b" }
        };

        var list = reg.List(ctx);
        Assert.Collection(list,
            item => Assert.Equal("a", item.Name));
    }

    [Fact]
    public void Resolve_UsesVersionSelector()
    {
        var reg = new InMemoryToolRegistry();
        var ctx = new ToolBindingContext { AgentId = "test" };

        reg.Publish(Make("t", "1.0.0"));
        reg.Publish(Make("t", "1.2.0"));

        var d = reg.Resolve("t", "^1.0.0", ctx);
        Assert.NotNull(d);
        Assert.Equal("1.2.0", d!.Version);
    }
}
