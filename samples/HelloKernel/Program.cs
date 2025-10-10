using AgentFramework.Engines.Loop;
using AgentFramework.Hosting;
using AgentFramework.Kernel;
using AgentFramework.Kernel.Policies.Defaults;
using AgentFramework.Runners.Timers;


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
            .WithKernel(() => new InProcKernelFactory())
            .AddEngine("loop", () => new LoopEngine("loop"))
            .AddRunner("loop", () => new TimerRunner(TimeSpan.FromSeconds(1), "Loop Tick"))
            .AddAgent("hello-loop", () => new HelloLoopAgent())
            .AddAgent("hello-mape", () => new HelloMapeAgent())
            .Attach("hello-loop", "loop")
            .Attach("hello-mape", "loop")
            .Build();

        await host.StartAsync();
        await Task.Delay(TimeSpan.FromSeconds(5));
        await host.StopAsync();

        Console.WriteLine("HelloKernel sample finished.");
    }
}
