using AgentFramework.Kernel;

namespace AgentFramework.Agents;

public abstract class AgentBase : IAgent
{
    public abstract string Id { get; }

    public virtual async Task HandleAsync(WorkItem item, IAgentContext ctx)
    {
        switch (item.Kind)
        {
            case WorkItemKind.Percept:
                await PerceiveAsync(item, ctx).ConfigureAwait(false);
                break;
            case WorkItemKind.Tick:
                await TickAsync(item, ctx).ConfigureAwait(false);
                break;
            default:
                ctx.Trace($"Unhandled WorkItem kind: {item.Kind}");
                break;
        }
    }

    protected virtual Task PerceiveAsync(WorkItem percept, IAgentContext ctx) => Task.CompletedTask;
    protected virtual Task TickAsync(WorkItem tick, IAgentContext ctx) => Task.CompletedTask;
}
