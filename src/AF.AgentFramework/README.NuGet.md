# AF.AgentFramework

An experimental agent framework for .NET:  
**Kernel**, **Engines**, **Runners**, **Hosting**, **Tools** and **MAS**.

> Target framework: **.NET 9.0**

## Install

```bash
dotnet add package AF.AgentFramework
```

## Quick start

```csharp
using System;
using System.Threading;
using System.Threading.Tasks;
using AgentFramework.Hosting;
using AgentFramework.Engines;
using AgentFramework.Runners.Timers;
using AgentFramework.Kernel;
using AgentFramework.Kernel.Policies.Defaults;

sealed class DummyAgent : IAgent
{
    public string Id => "dummy";
    public async Task HandleAsync(WorkItem item, IAgentContext ctx)
    {
        Console.WriteLine($"[Agent] {Id} handling {item.Kind} ({item.Id})");
        await Task.Delay(100, ctx.Cancellation);
    }
}

var host = AgentHostBuilder.Create()
    .WithKernelDefaults(PolicySetDefaults.Create())
    .AddEngine("loop", () => new LoopEngine("loop"))
    .AddRunner("loop", () => new TimerRunner(TimeSpan.FromSeconds(1), "Loop Tick"))
    .AddAgent("dummy", () => new DummyAgent())
    .Attach("dummy", "loop")
    .Build();

await host.StartAsync();
await Task.Delay(TimeSpan.FromSeconds(5));
await host.StopAsync();
```

## Links

- Source & issues: https://github.com/runemalm/AF.AgentFramework  
- API docs (DocFX): see the repository’s `docs/` site

## License

MIT © David Runemalm
