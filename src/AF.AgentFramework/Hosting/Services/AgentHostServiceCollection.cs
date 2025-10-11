namespace AgentFramework.Hosting.Services;

public sealed class AgentHostServiceCollection
{
    public List<Func<IAgentHostService>> Factories { get; } = new();
}
