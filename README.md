# AF.AgentFramework

\> A minimal, extensible **agent runtime** for .NET — built around a small set of primitives:  
\> **Kernel**, **Engines**, **Runners**, **Hosting**, **Agents**, and **Policies**.

[![NuGet version](https://img.shields.io/nuget/v/AgentFramework.Kernel.svg)](https://www.nuget.org/packages/AgentFramework.Kernel/)
[![Build status](https://github.com/runemalm/AgentFramework.Kernel/actions/workflows/release.yml/badge.svg?branch=master)](https://github.com/runemalm/AgentFramework.Kernel/actions/workflows/release.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
![Target](https://img.shields.io/badge/.NET-9.0-512BD4)

## Goals

- **Small core, strong contracts** — easy to reason about and test.  
- **Deterministic execution** — SingleActive per agent, policy-driven behavior.  
- **Pluggable engines/runners** — timers, webhooks, queues, cron, etc.  
- **Composability via Hosting** — wire agents↔engines with overrides.  

## Packages / Namespaces

Single project for now (to be split later):

- `AgentFramework.Kernel` — contracts, dispatcher, policies  
- `AgentFramework.Engines` — engine contracts + built-ins (Loop, Reactive)  
- `AgentFramework.Runners` — runner contracts + built-ins (Timer)  
- `AgentFramework.Hosting` — host + builder for composition  
- `AgentFramework.Kernel.Policies.*` — policy contracts \& defaults  

## Architecture

- **Kernel** — per-agent mailbox, **SingleActive**, ordering, admission, timeout, retry, cooperative preemption, backpressure.  
- **Engine** — receives events/ticks, turns them into `WorkItem`s, enqueues to Kernel.  
- **Runner** — I/O or timing source owned by an engine.  
- **Host** — composes everything, manages lifecycle.  
- **Policies** — swappable behaviors (Admission, Ordering, Preemption, Retry, Timeout, Backpressure).  

## Quick start

Install the package:

```bash
dotnet add package AF.AgentFramework
```

Create an agent and wire it up with a loop engine and a timer runner:

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

var defaults = PolicySetDefaults.Create(
    retry: RetryOptions.Default with { MaxAttempts = 2 }
);

var host = AgentHostBuilder.Create()
    .WithKernelDefaults(defaults)
    .AddEngine("loop", () => new LoopEngine("loop"))
    .AddRunner("loop", () => new TimerRunner(TimeSpan.FromSeconds(1), "Loop Tick"))
    .AddAgent("dummy", () => new DummyAgent())
    .Attach("dummy", "loop")
    .Build();

await host.StartAsync();
await Task.Delay(TimeSpan.FromSeconds(5));
await host.StopAsync();
```

Run the included sample instead:

```bash
make hello-kernel
```

## Policies (defaults)

- **Admission**: accept unless queue is too long; defer/reject late items.  
- **Ordering**: priority → deadline → stable tiebreak.  
- **Preemption**: off by default (cooperative supported).  
- **Retry**: exponential backoff, no retry on cancellation.  
- **Timeout**: opt-in; cancels agent handler when exceeded.  
- **Backpressure**: Normal / Throttle / Shed (shed drops enqueues).  

Override a single policy via an **attachment** or set global defaults:

```csharp
var defaults = PolicySetDefaults.Create(
    timeout: new TimeoutOptions(TimeSpan.FromMilliseconds(250))
);
```

## 🧪 Samples & Tests

- **[samples/HelloKernel](samples/HelloKernel)** – minimal hello world sample app.
- **[tests/AgentFramework.Kernel.Tests](tests/AgentFramework.Kernel.Tests)** – xUnit tests, currently only InProcKernel tests

Run tests:

```bash
make test
```

Run the sample:

```bash
make hello-kernel
```

## Versioning & Releases

- Versions are managed by [semantic-release](https://semantic-release.gitbook.io/) and git tags (`vX.Y.Z`).
- Pre-1.0: **breaking changes bump minor**, not major (`preMajor:true`).
- Package versions are injected at build time via [MinVer](https://github.com/adamralph/minver).
- See [CHANGELOG.md](CHANGELOG.md) for release history.

Dry-run a release locally:

```bash
make release-dry-run
```

## Contributing

1. Fork & clone  
2. Run `make build` / `make test`  
3. Add features with Conventional Commit messages (`feat: ...`, `fix: ...`)  
4. Open a PR

## 📄 License

MIT © [David Runemalm](https://github.com/runemalm)
