using AgentFramework.Tools.Registry;
using Xunit;

namespace AgentFramework.Tests.Tools;

public class VersionSelectorTests
{
    [Fact]
    public void Select_Highest_WhenRangeNull()
    {
        var versions = new List<string> { "1.0.0", "1.2.0", "0.9.9" };
        var selected = VersionSelector.Select(versions, null);
        Assert.Equal("1.2.0", selected);
    }

    [Fact]
    public void Select_Exact_Match()
    {
        var versions = new List<string> { "1.0.0", "1.2.0" };
        var selected = VersionSelector.Select(versions, "1.0.0");
        Assert.Equal("1.0.0", selected);
    }

    [Fact]
    public void Select_CaretRange_SameMajorHighest()
    {
        var versions = new List<string> { "1.0.0", "1.2.0", "2.0.0" };
        var selected = VersionSelector.Select(versions, "^1.0.0");
        Assert.Equal("1.2.0", selected);
    }

    [Fact]
    public void Select_Wildcard_Major()
    {
        var versions = new List<string> { "1.1.0", "2.0.0", "1.3.5" };
        var selected = VersionSelector.Select(versions, "1.*");
        Assert.Equal("1.3.5", selected);
    }

    [Fact]
    public void Select_Wildcard_MajorMinor()
    {
        var versions = new List<string> { "1.2.0", "1.2.7", "1.3.0" };
        var selected = VersionSelector.Select(versions, "1.2.*");
        Assert.Equal("1.2.7", selected);
    }

    [Fact]
    public void Select_Throws_WhenNoMatch()
    {
        var versions = new List<string> { "1.0.0" };
        Assert.Throws<InvalidOperationException>(() => VersionSelector.Select(versions, "^2.0.0"));
    }
}