using AgentFramework.Agents;
using AgentFramework.Kernel;

namespace HelloKernel.Agents;

sealed class HelloReactiveAgent : AgentBase
{
    public override string Id => "hello-reactive";

    protected override Task PerceiveAsync(WorkItem percept, IAgentContext ctx)
    {
        ctx.Trace($"[Perceive] Received percept → {percept.Payload ?? "(no payload)"}");

        // Simulate reasoning or acting upon percept
        if (percept.Payload is string message)
            Console.WriteLine($"[Agent] {Id} reacting to: \"{message}\"");
        else
            Console.WriteLine($"[Agent] {Id} received unknown percept.");

        return Task.CompletedTask;
    }
}
