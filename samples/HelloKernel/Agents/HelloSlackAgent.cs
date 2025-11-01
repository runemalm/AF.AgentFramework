using AgentFramework.Agents;
using AgentFramework.Kernel;
using AgentFramework.Kernel.Routing;
using HelloKernel.Runners;

namespace HelloKernel.Agents;

/// <summary>
/// Demonstrates reactive perception and the FilteredStimulusRouter.
/// This agent only receives percepts whose topic matches the declared filters.
/// </summary>
[PerceptFilter("slack/message")]
[PerceptFilter("slack/reaction")]
sealed class HelloSlackAgent : AgentBase
{
    public override string Id => "hello-slack";

    protected override Task PerceiveAsync(WorkItem percept, IAgentContext ctx)
    {
        if (percept.Payload is not SlackEvent ev)
            return Task.CompletedTask;

        ctx.Trace($"[Agent] {Id} received {ev.Topic} from {ev.User}: {ev.Text}");
        return Task.CompletedTask;
    }
}
