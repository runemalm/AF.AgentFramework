# AgentFramework.Kernel

Minimal agent kernel for .NET 8 with a step-based middleware pipeline.

---

## Install

```bash
dotnet add package AgentFramework.Kernel
```

---

## Quick Start

```csharp
using AgentFramework.Kernel;
using AgentFramework.Kernel.Abstractions;
using AgentFramework.Kernel.Profiles;

var profile = new TestProfile();
var kernel = new AgentKernel(profile, new IKernelMiddleware[]
{
    new LoggingMiddleware()
});

await kernel.StartAsync();
await kernel.TickAsync();
await kernel.StopAsync();
```

---

## Concepts

- **Profiles**: Define the ordered steps of a control loop.  
- **Middleware**: Wrap each step for logging, metrics, retries, etc.  
- **Context**: Provides StepId, CancellationToken, and Items dictionary.

---

## Learn More

Repository: [https://github.com/runemalm/AgentFramework.Kernel](https://github.com/runemalm/AgentFramework.Kernel)
Sample: [samples/HelloKernel](https://github.com/runemalm/AgentFramework.Kernel/tree/master/samples/HelloKernel)

---

## License

MIT © David Runemalm
