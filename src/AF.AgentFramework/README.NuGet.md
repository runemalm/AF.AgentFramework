# AF.AgentFramework

A lightweight, theory-aligned **agent framework for .NET** —
build autonomous agents and multi-agent systems (MAS) with clean, composable primitives:
**Kernel · Engines · Runners · Hosting · Tools**.

> Requires .NET 9.0 or later

## Install

```bash
dotnet add package AF.AgentFramework
```

## Quick start

Minimal example:

```csharp
using AgentFramework.Hosting;
using AgentFramework.Engines.Loop;
using AgentFramework.Runners.Timers;
using AgentFramework.Kernel;
using AgentFramework.Kernel.Policies.Defaults;

sealed class HelloLoopAgent : IAgent
{
    public string Id => "hello-loop";
    
    public async Task HandleAsync(WorkItem item, IAgentContext ctx)
    {
        Console.WriteLine($"[Agent] {Id} handling {item.Kind}");
        await Task.Delay(100, ctx.Cancellation);
    }
}

var host = AgentHostBuilder.Create()
    .WithKernelDefaults(PolicySetDefaults.Create())
    .WithKernel(() => new InProcKernelFactory())
    .AddEngine("loop", () => new LoopEngine("loop"))
    .AddRunner("loop", () => new TimerRunner(TimeSpan.FromSeconds(1)))
    .AddAgent("hello-loop", () => new HelloLoopAgent())
    .Attach("hello-loop", "loop")
    .Build();

await host.StartAsync();
await Task.Delay(5000);
await host.StopAsync();
```

## Links

- [Source & issues](https://github.com/runemalm/AF.AgentFramework)
- API docs (DocFX): see `docs/` in the repository

## License

MIT © David Runemalm
