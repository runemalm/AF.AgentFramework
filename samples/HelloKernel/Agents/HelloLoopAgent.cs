using AgentFramework.Agents;
using AgentFramework.Kernel;

namespace HelloKernel.Agents;

sealed class HelloLoopAgent : AgentBase
{
    public override string Id => "hello-loop";

    protected override async Task TickAsync(WorkItem tick, IAgentContext ctx)
    {
        Console.WriteLine($"[Agent] {Id} Tick ({tick.Id}) - \"Hello World!\"");
        await Task.Delay(100, ctx.Cancellation);

        // Uncomment to test retry behavior: throw once every 3rd tick
        if (DateTimeOffset.UtcNow.Second % 3 == 0)
            throw new InvalidOperationException("Simulated failure");
    }
}
