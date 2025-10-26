using AgentFramework.Agents;
using AgentFramework.Kernel;
using AgentFramework.Mas.Features;

namespace HelloKernel.Agents;

/// <summary>
/// HelloAwareAgent — demonstrates early MAS awareness.
/// It perceives other agents via the Directory, observes shared facts on the Blackboard,
/// and posts its own status each tick.
/// </summary>
sealed class HelloAwareAgent : AgentBase
{
    public override string Id => "hello-aware";

    protected override Task TickAsync(WorkItem item, IAgentContext ctx)
    {
        ctx.Trace("Tick → observing environment...");

        var dir = ctx.Features.Get<IMasDirectory>();
        var bb  = ctx.Features.Get<IMasBlackboard>();

        // Observe society (social awareness)
        var agents = dir.ListAll();
        var agentList = string.Join(", ", agents.Select(a => a.AgentId));
        ctx.Trace($"Directory sees {agents.Count} registered agents: {agentList}");

        // Post own existence (environmental awareness)
        bb.Post($"{Id}-status", "I exist.");

        // Observe environment (blackboard awareness)
        var facts = bb.Enumerate();
        ctx.Trace($"Blackboard currently has {facts.Count} facts.");

        return Task.CompletedTask;
    }
}
