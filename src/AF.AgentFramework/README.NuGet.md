# AF.AgentFramework

A minimal, extensible **agent runtime** for .NET:  
**Kernel**, **Engines**, **Runners**, **Hosting**, and **Policies**.

- **Deterministic:** SingleActive per agent with ordered queues.  
- **Policy-driven:** Admission, Ordering, Timeout, Retry, Preemption, Backpressure.  
- **Composable:** Attach agents to engines with per-attachment overrides.  
- **Extensible:** Plug your own engines/runners.  

\> Target framework: **.NET 9.0**

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

## Policies (defaults)

- Admission: accept unless queue is too long; reject late items.  
- Ordering: priority first, then FIFO/deadline.  
- Timeout: disabled by default (set via `TimeoutOptions`).  
- Retry: exponential backoff; no retry on cancellations.  
- Preemption: cooperative (opt-in).  
- Backpressure: throttle/shed at high load.  

## Links

- Source \& issues: https://github.com/runemalm/AF.AgentFramework  
- API docs (DocFX): see the repository’s `docs/` site  

MIT © David Runemalm

## License

MIT © David Runemalm
