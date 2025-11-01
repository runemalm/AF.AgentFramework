using AgentFramework.Engines.Loop;
using AgentFramework.Engines.Reactive;
using AgentFramework.Hosting;
using AgentFramework.Kernel;
using AgentFramework.Kernel.Policies.Defaults;
using AgentFramework.Kernel.Routing;
using AgentFramework.Mas.Integration;
using AgentFramework.Runners.Timers;
using AgentFramework.Tools.Integration;
using HelloKernel.Agents;
using HelloKernel.Runners;
using HelloKernel.Tools;
using Microsoft.Extensions.DependencyInjection;

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
            .AddEngine("reactive", _ => new ReactiveEngine("reactive", new BroadcastStimulusRouter()))
            .AddRunner("reactive", () => new HttpMockRunner())
            .AddAgent("hello-reactive", () => new HelloReactiveAgent())
            .Attach("hello-reactive", "reactive")
            // demo of tools subsystem
            .AddAgent("hello-tools", () => new HelloToolsAgent())
            .Attach("hello-tools", "loop")
            .AddTools()
            .AddLocalTool<LocalEchoTool>()
            // demo of aware agents (using minimal MAS capability)
            .AddAgent("hello-aware", () => new HelloAwareAgent())
            .Attach("hello-aware", "loop")
            .AddMas()
            // demo of specialized reactive engine using filtered router
            .AddEngine("slack-reactive", sp =>
                new ReactiveEngine("slack-reactive", sp.GetRequiredService<FilteredStimulusRouter>()))
            .AddRunner("slack-reactive", () => new SlackMockRunner())
            .AddAgent("hello-slack", () => new HelloSlackAgent())
            .Attach("hello-slack", "slack-reactive")
            // add the live dashboard
            .EnableDashboard(6060)
            .Build();
        
        await host.RunConsoleAsync();

        Console.WriteLine("HelloKernel sample finished.");
    }
}
