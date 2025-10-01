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
- Building minimal examples (e.g. `HelloKernel` and upcoming `HelloAgent` and `HelloSocietyAgent`)

APIs are **not stable yet**. Expect things to change as I refine the abstractions.

## Roadmap

- [x] Basic agent kernel scaffolding
- [x] Loop and reactive engines scaffolding
- [x] Tools (agent-usable capabilities, e.g. external actions) scaffolding
- [ ] MAPE-K execution semantics scaffolding (thorough loop engine "profiles")
- [ ] MAS primitives scaffolding (blackboard, directories, collaboration, ...)
- [ ] Runner ecosystem scaffolding (e.g. Slack Webhooks, timers, HTTP ingress, etc..)
- [ ] Documentation resources scaffolding (DocFX, API reference and articles)
- [ ] After scaffolding of above, finish/polish it for v1.0.0
- [ ] First public v1.0.0 NuGet release!

## Contributing

I would love to collaborate with like-minded on this. If you are interested, please reach out.      
Or, if you just find this interesting:
- ⭐ **Star this repo** to follow along
- 👀 Watch the releases for updates
- 💬 Open discussions or issues if you have ideas or feedback

## License

This project is licensed under the [MIT License](LICENSE).

*I’m building AF.AgentFramework to learn, but also hoping it can become a useful resource for others who want to explore agent-based architectures in .NET. If that sounds interesting, please give it a star and follow along!*
