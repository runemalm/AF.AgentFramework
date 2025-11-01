using AgentFramework.Kernel.Routing;
using Xunit;

namespace AgentFramework.Tests.Kernel.Routing;

public sealed class TopicPatternMatcherTests
{
    [Theory]
    [InlineData("slack/message", "slack/message", true)]   // exact match
    [InlineData("slack/message", "slack/*", true)]         // single-level wildcard
    [InlineData("slack/message/posted", "slack/*", false)] // deeper level shouldn't match
    [InlineData("slack/message/posted", "slack/#", true)]  // recursive wildcard
    [InlineData("sensor/temperature/outdoor", "sensor/#", true)]
    [InlineData("sensor/temperature/outdoor", "sensor/*", false)]
    [InlineData("any/topic", "*", true)]                   // match-all
    [InlineData("SLACK/Message", "slack/message", true)]   // case-insensitive
    [InlineData("slack", "slack/message", false)]          // not a prefix match unless wildcard
    [InlineData("slack/message", "slack", false)]          // reverse case
    public void Matches_TopicPatterns_BehaveAsExpected(string topic, string pattern, bool expected)
    {
        // Act
        var result = TopicPatternMatcher.Matches(topic, pattern);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(null, "slack/*", false)]
    [InlineData("", "slack/*", false)]
    [InlineData("slack/message", null, false)]
    [InlineData("slack/message", "", false)]
    public void Matches_InvalidInputs_ReturnsFalse(string? topic, string? pattern, bool expected)
    {
        Assert.Equal(expected, TopicPatternMatcher.Matches(topic!, pattern!));
    }

    [Fact]
    public void Matches_RecursiveAndSingleLevelBehaveDifferently()
    {
        // slack/* should not match multi-level topics
        Assert.False(TopicPatternMatcher.Matches("slack/message/posted", "slack/*"));

        // slack/# should match recursively
        Assert.True(TopicPatternMatcher.Matches("slack/message/posted", "slack/#"));
    }
}