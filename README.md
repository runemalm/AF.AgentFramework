# AF.AgentFramework

[![NuGet version](https://img.shields.io/nuget/v/AF.AgentFramework.svg)](https://www.nuget.org/packages/AF.AgentFramework/)    
[![Build status](https://github.com/runemalm/AF.AgentFramework/actions/workflows/release.yml/badge.svg?branch=master)](https://github.com/runemalm/AF.AgentFramework/actions/workflows/release.yml)    
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)    
![Target](https://img.shields.io/badge/.NET-9.0-512BD4)
    
---

**AF.AgentFramework** is an experimental .NET framework for building **agents** and, eventually, **multi-agent systems (MAS)**.      
It’s a personal learning project — but also intended to grow into a reusable toolkit for others who want to explore agent-based architectures in C#/.NET.

The framework is rooted in **agent theory and MAS research**. My goal is to create clean, theory-aligned abstractions for:
- **Kernel** – the minimal agent core
- **Engines** – execution models (loop, reactive, etc.)
- **Runners** – input/output adapters that connect agents to their environment
- **Tools** – agent-usable capabilities, e.g. external actions
- **Hosting** – integration with .NET GenericHost and application lifecycles
- **MAS** – (planned) support for multi-agent collaboration

---

## Why?

Agents and MAS have been studied for decades in AI research, but they’re rarely accessible to everyday .NET developers.      
I want to **bridge theory and practice** — building a framework that is simple enough to learn from, yet solid enough to use for real projects.

This repository is where I’m exploring:
- How agentic abstractions map to practical .NET patterns
- How concepts like **roles, policies, perception, and action** can be modeled in code
- How multi-agent collaboration can be layered on top of a clean agent kernel

---

## Current Status

🚧 **Work in Progress** 🚧 Right now the focus is on:
- Laying down scaffolding and core abstractions
- Building minimal examples (e.g. `HelloKernel`)
- Experimenting with agent engines and failure policies

APIs are **not stable yet**. Expect things to change as I refine the abstractions.
    
---

## Roadmap

- [x] Basic agent kernel scaffold
- [x] Loop and reactive engines scaffold
- [x] Tools (agent-usable capabilities, e.g. external actions) scaffold
- [ ] MAPE-K execution semantics through loop engine profile scaffold
- [ ] MAS primitives (blackboard, directories, collaboration) scaffold
- [ ] Runner ecosystem (e.g. Slack, timers, HTTP ingress) scaffold
- [ ] Documentation site (DocFX) scaffold
- [ ] Fully implement all of above scaffolding
- [ ] First public v1.0.0 NuGet release

---

## Contributing

Since this is still exploratory, I’m not actively accepting large PRs yet.      
But if you find it interesting:
- ⭐ **Star this repo** to follow along
- 👀 Watch the releases for updates
- 💬 Open discussions or issues if you have ideas or feedback

---

## License

This project is licensed under the [MIT License](LICENSE).
    
---

*I’m building AF.AgentFramework to learn, but also hoping it can become a useful resource for others who want to explore agent-based architectures in .NET. If that sounds interesting, please give it a star and follow along!*
