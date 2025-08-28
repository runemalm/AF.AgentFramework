namespace AgentFramework.Agent;

public sealed class AgentContext
{
    public AgentId AgentId { get; }
    public IList<Message> Conversation { get; }
    public IToolRegistry Tools { get; }
    public IMemoryStore Memory { get; }
    public IMetadataBag Metadata { get; }
    public CancellationToken Cancellation { get; }

    public AgentContext(
        AgentId agentId,
        IList<Message> conversation,
        IToolRegistry tools,
        IMemoryStore memory,
        IMetadataBag metadata,
        CancellationToken cancellation = default)
    {
        AgentId = agentId;
        Conversation = conversation;
        Tools = tools;
        Memory = memory;
        Metadata = metadata;
        Cancellation = cancellation;
    }
}
