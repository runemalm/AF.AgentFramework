using AgentFramework.Engines.Loop;
using AgentFramework.Engines.Reactive;
using AgentFramework.Hosting;
using AgentFramework.Kernel;
using AgentFramework.Kernel.Policies.Defaults;
using AgentFramework.Runners.Timers;
using AgentFramework.Tools.Integration;
using HelloKernel.Agents;
using HelloKernel.Tools;

namespace HelloKernel;

internal class Program
{
    private static async Task Main()
    {
        Console.WriteLine("HelloKernel sample starting…");

        var policyDefaults = PolicySetDefaults.Create(
            retry: RetryOptions.Default with { MaxAttempts = 2 },
            timeout: new TimeoutOptions(null),
            scheduling: new TimeSliceAwareSchedulingPolicy()
        );

        var host = AgentHostBuilder.Create()
            .WithKernelDefaults(policyDefaults)
            .WithKernelConcurrency(2)
            .WithKernel(() => new InProcKernelFactory())
            // loop family
            .AddEngine("loop", () => new LoopEngine("loop"))
            .AddRunner("loop", () => new TimerRunner(TimeSpan.FromSeconds(1.0), "Loop Tick"))
            .AddAgent("hello-loop", () => new HelloLoopAgent())
            .AddAgent("hello-mape", () => new HelloMapeAgent())
            .Attach("hello-loop", "loop")
            .Attach("hello-mape", "loop")
            // reactive family
            .AddEngine("reactive", () => new ReactiveEngine("reactive"))
            .AddRunner("reactive", () => new HttpMockRunner())
            .AddAgent("hello-reactive", () => new HelloReactiveAgent())
            .Attach("hello-reactive", "reactive")
            // tools subsystem demo
            .AddAgent("hello-tools", () => new HelloToolsAgent())
            .Attach("hello-tools", "loop")
            .AddTools()
            .AddLocalTool<LocalEchoTool>()
            // add the live dashboard
            .EnableDashboard(6060)
            .Build();
        
        await host.RunConsoleAsync();

        Console.WriteLine("HelloKernel sample finished.");
    }
}
