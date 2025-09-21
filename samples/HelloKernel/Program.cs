using AgentFramework.Engines.Loop;
using AgentFramework.Hosting;
using AgentFramework.Kernel;
using AgentFramework.Kernel.Policies.Defaults;
using AgentFramework.Runners.Timers;

sealed class HelloAgent : IAgent
{
    public string Id { get; } = "hello";

    public async Task HandleAsync(WorkItem item, IAgentContext ctx)
    {
        // Simulate a small bit of work
        Console.WriteLine($"[Agent] {Id} handling {item.Kind} ({item.Id}) - \"Hello World!\"");
        await Task.Delay(100, ctx.Cancellation);

        // Uncomment to test retry behavior: throw once every 3rd tick
        if (DateTimeOffset.UtcNow.Second % 3 == 0)
            throw new InvalidOperationException("Simulated failure");
    }
}

internal class Program
{
    private static async Task Main()
    {
        Console.WriteLine("HelloKernel sample starting…");

        var policyDefaults = PolicySetDefaults.Create(
            retry: RetryOptions.Default with { MaxAttempts = 2 },
            timeout: new TimeoutOptions(null)
        );

        var host = AgentHostBuilder.Create()
            .WithKernelDefaults(policyDefaults)
            .AddEngine("loop", () => new LoopEngine("loop"))
            .AddRunner("loop", () => new TimerRunner(TimeSpan.FromSeconds(1), "Loop Tick"))
            .AddAgent("hello", () => new HelloAgent())
            .Attach("hello", "loop")
            .Build();

        await host.StartAsync();
        await Task.Delay(TimeSpan.FromSeconds(5));
        await host.StopAsync();

        Console.WriteLine("HelloKernel sample finished.");
    }
}
