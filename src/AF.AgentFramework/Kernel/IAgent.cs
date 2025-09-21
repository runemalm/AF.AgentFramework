namespace AgentFramework.Kernel;

public interface IAgent
{
    string Id { get; }
    Task HandleAsync(WorkItem item, IAgentContext context);
}
