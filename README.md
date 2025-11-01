# AF.AgentFramework

[![NuGet version](https://img.shields.io/nuget/v/AF.AgentFramework.svg)](https://www.nuget.org/packages/AF.AgentFramework/)
[![Build status](https://github.com/runemalm/AF.AgentFramework/actions/workflows/release.yml/badge.svg?branch=master)](https://github.com/runemalm/AF.AgentFramework/actions/workflows/release.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
![Target](https://img.shields.io/badge/.NET-9.0-512BD4)

**AF.AgentFramework** is an experimental .NET framework for building **agents** and **multi-agent systems (MAS)**.
It’s a personal learning project — but I expect it to also grow into a fully-fledged framework for others who want to both explore/learn agent-based architectures in C#/.NET and to build agentic systems for production.

The framework is rooted in **agent theory and MAS research**. My goal is to create clean, theory-aligned abstractions for:
- **Kernel** – the minimal agent core
- **Engines** – execution models (loop, reactive, etc.)
- **Runners** – input adapters that generates engine "ticks"
- **Tools** – agent-usable capabilities, e.g. external actions
- **Hosting** – integration with .NET GenericHost and application lifecycles
- **MAS** – support for multi-agent collaboration using blackboards and agent directories
- **Capabilities** – modular extensions that attach to the agent context (MAS, Observability, MCP, etc.)

## Why?

There are many frameworks for creating agentic systems, like e.g. langgraph, openai swarm, etc. just to name a couple. What I think they all lack is alignment with the agentic theory. At least if you are new to agentic architectures and are starting out.

I want to **bridge theory and practice** — building a framework that is simple enough to learn from, yet solid enough to use for real projects.

This repository is where I’m exploring:
- How agentic abstractions map to practical agent architectures, integrating with and using .NET patterns
- How concepts like **MAPE-K, Tools/Actuators, Policies, MAS and Agent Societies** can be implemented in code

## Current Status

🚧 **Work in Progress** 🚧 Right now the focus is on:
- Laying down scaffolding and core abstractions (the theory "spine")
- Adding all the base modules (Kernel, Engines, Runners, Hosting, Tools, MultiAgent)
- Building minimal examples (e.g. `HelloLoopAgent`, `HelloMapeAgent`, and `HelloReactiveAgent`) all running together in the `HelloKernel` sample

APIs are **not stable yet**. Expect things to change as I refine the abstractions.

The new **Tools subsystem** is now integrated end-to-end — agents can discover and invoke local tools through the framework’s tool pipeline.

The framework now includes a **pluggable metrics provider system**, enabling consistent real-time observability of agents and tools.
It’s designed to be extensible — future integrations may bridge these metrics into the .NET diagnostics ecosystem (e.g., System.Diagnostics.Metrics, OpenTelemetry, or other observability backends).

The framework now has a **capability-based extensibility system** — a foundation that lets modules like MAS, Observability, and MCP attach cleanly to the agent context without modifying the kernel.  
Capabilities are registered through the host builder and automatically injected into each agent context at runtime.

The framework now introduces a **Stimulus Router** primitive — a flexible routing layer between reactive engines and agents.
Routers decide which agents should receive each percept or external event. The default `BroadcastStimulusRouter` sends all percepts to all attached agents, while the `FilteredStimulusRouter` uses topic-based filters declared via `[PerceptFilter("topic")]` attributes on agents.

### Multi-Agent System (MAS) subsystem

The **MAS capabilities** provide shared collaboration primitives across agents:
- A **Directory** for registering and discovering agents (the “yellow pages” of the system)
- A **Blackboard** for posting and reading shared facts (the shared environment)

These are attached as capabilities via `AddMas()` and are globally shared within a host.
The included `HelloAwareAgent` sample demonstrates early *social* and *environmental awareness* using these MAS primitives.

## Live Dashboard

When running the included **HelloKernel** sample (with `HelloLoopAgent`, `HelloReactiveAgent`, and `HelloMapeAgent`),
you can open the minimal built-in dashboard at [http://localhost:6060/af](http://localhost:6060/af)
to observe agent activity and kernel state in real time.

<p align="center">
  <img src="docs/images/dashboard-sample.png" alt="AF.AgentFramework Dashboard showing live agent metrics" width="800">
</p>

The dashboard is powered by the framework’s own **ObservabilityServer**, exposing live metrics from the kernel
through a lightweight HTTP interface. It visualizes throughput, queue depth, utilization, and per-agent status — now including live tool metrics (invocations, errors, and last tool used).

## HelloKernel Sample

The `HelloKernel` project demonstrates how multiple agents and engines run together
in a single host — including **loop**, **reactive**, and **MAPE-K** agents.

```csharp
using AgentFramework.Engines.Loop;
using AgentFramework.Engines.Reactive;
using AgentFramework.Hosting;
using AgentFramework.Kernel;
using AgentFramework.Kernel.Policies.Defaults;
using AgentFramework.Mas.Integration;
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

        // Build and run a minimal multi-agent host
        var host = AgentHostBuilder.Create()
            .WithKernelDefaults(policyDefaults)
            .WithKernelConcurrency(2)
            .WithKernel(() => new InProcKernelFactory())
            // loop family
            .AddEngine("loop", () => new LoopEngine("loop"))
            .AddRunner("loop", () => new TimerRunner(TimeSpan.FromSeconds(0.7), "Loop Tick"))
            .AddAgent("hello-loop", () => new HelloLoopAgent())
            .AddAgent("hello-mape", () => new HelloMapeAgent())
            .Attach("hello-loop", "loop")
            .Attach("hello-mape", "loop")
            // reactive family
            .AddEngine("reactive", () => new ReactiveEngine("reactive"))
            .AddRunner("reactive", () => new HttpMockRunner())
            .AddAgent("hello-reactive", () => new HelloReactiveAgent())
            .Attach("hello-reactive", "reactive")
            // agents can use tools
            .AddAgent("hello-tools", () => new HelloToolsAgent())
            .Attach("hello-tools", "loop")
            .AddTools()
            .AddLocalTool<LocalEchoTool>()
            // agents are becoming aware
            .AddAgent("hello-aware", () => new HelloAwareAgent())
            .Attach("hello-aware", "loop")
            .AddMas()
            // agents can receive filtered percepts via stimulus routers
            .AddEngine("slack-reactive", sp => new ReactiveEngine("slack-reactive", sp.GetRequiredService<FilteredStimulusRouter>()))
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
```

🧩 Try running it — then open the [dashboard](http://localhost:6060/af) to see all agents executing live.

## HelloToolsAgent Sample

The `HelloToolsAgent` demonstrates how agents can invoke external **tools** via the framework’s `Tools` subsystem.
In this sample, the agent calls a simple `LocalEchoTool` to produce an output through the full pipeline (validation → authorization → policy → execution → postprocessing).

```text
[Context:hello-tools] [Agent] Loop #1 — invoking tool...
[Context:hello-tools] [Agent] Tool OK → "echo: hello world"
```

The tool registry is automatically initialized at startup and visible to all agents that include `AddTools()` and `AddLocalTool<...>()` in the host builder.

## HelloAwareAgent Sample

The new `HelloAwareAgent` demonstrates how agents can use shared **MAS capabilities** — the `Directory` and `Blackboard` — to gain awareness of other agents and their shared environment.

```text
[Context:hello-aware] Tick → observing environment...
[Context:hello-aware] Directory sees 5 registered agents: hello-tools, hello-loop, hello-aware, hello-reactive, hello-mape
[Context:hello-aware] Blackboard currently has 1 facts.
```

These capabilities are enabled by adding `.AddMas()` to the host builder, which attaches the shared Directory and Blackboard to all agents in the system.

## HelloSlackAgent Sample

The `HelloSlackAgent` demonstrates how the new **FilteredStimulusRouter** can be used to selectively route percepts based on their topics.

Each agent can declare its interest in specific percepts using the `[PerceptFilter]` attribute:

```csharp
[PerceptFilter("slack/message")]
[PerceptFilter("slack/reaction")]
sealed class HelloSlackAgent : AgentBase { … }
```

When running the sample, only the `HelloSlackAgent` receives simulated Slack events like `"slack/message"` and `"slack/reaction"`, routed through the filtered router of the `slack-reactive` engine.

## Roadmap

This is a growing list and subject to change as we go and learn.

### 🧱 Foundation
Core architectural scaffolding — defining the minimal abstractions for agents, engines, and runners.

- [x] Implement core **agent kernel** (InProcKernel, policies, scheduling)
- [x] Implement **execution engines**: LoopEngine & ReactiveEngine
- [x] Implement **runner primitives**: TimerRunner, ReactiveRunner

### 🧩 Capabilities & Collaboration
Expanding what agents can *do* — tools, feedback loops, collaboration, and environmental interaction.

- [x] Add **Tools system** (external actions, pipelines, tool engine, policies, observability, ...)
- [x] Add **MAPE-K agent base** (`MapekAgentBase`) and sample
- [x] Add **Capability system** (foundation for MAS, Observability, MCP)
- [x] Add **minimal MAS subsystem** (directory + blackboard primitives)
- [x] Add **Stimulus Router** primitive (Broadcast + Filtered routing)
- [ ] Add **webhook percept** full support
- [ ] Add **slack percept** full support
- [ ] Add **MCP** support

### 🤖 Samples & Agents
Demonstrating theory in practice — progressively complex agents showcasing different patterns.

- [x] **HelloLoopAgent** – basic periodic agent using LoopEngine + TimerRunner
- [x] **HelloReactiveAgent** – event-driven agent using ReactiveEngine + HttpMockRunner
- [x] **HelloMapeAgent** – agent demonstrating the full MAPE-K control loop
- [x] **HelloToolsAgent** – agent invoking local tools via the Tools subsystem
- [x] **HelloAwareAgent** – demonstrates basic multi-agent awareness using Directory + Blackboard
- [x] **HelloSlackAgent** – showcases topic-based routing with FilteredStimulusRouter
- [ ] **HelloSocietyAgent** – sample MAS scenario (collaborating agents via blackboard)

### 🔍 Observability & Dashboard
Making the invisible visible — introspection and live visualization of agent activity and kernel state.

- [x] Add **pluggable metrics provider system** for agent and tool observability
- [x] Extend **dashboard** with per-agent tool metrics columns

### 📚 Ecosystem & Release
Documentation, polish, and packaging toward a stable v1.0 developer experience.

- [ ] Add **documentation site** (DocFX, API reference, guides)
- [ ] Finalize **v1.0.0 release** (stabilize APIs, add samples & polish)

## Contributing

I would love to collaborate with like-minded on this. If you are interested, please reach out.
Or, if you just find this interesting:
- ⭐ **Star this repo** to follow along
- 👀 Watch the releases for updates
- 💬 Open discussions or issues if you have ideas or feedback

## License

This project is licensed under the [MIT License](LICENSE).

*I’m building AF.AgentFramework to learn, but also hoping it can become a useful resource for others who want to explore agent-based architectures in .NET. If that sounds interesting, please give it a star and follow along!*
