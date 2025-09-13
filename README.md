# AgentFramework.Kernel

[![NuGet version](https://img.shields.io/nuget/v/AgentFramework.Kernel.svg)](https://www.nuget.org/packages/AgentFramework.Kernel/)
[![Build status](https://github.com/runemalm/AgentFramework.Kernel/actions/workflows/release.yml/badge.svg?branch=master)](https://github.com/runemalm/AgentFramework.Kernel/actions/workflows/release.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

**AgentFramework.Kernel** is a minimal .NET 8 library for building agents based on
the [MAPE-K loop](https://en.wikipedia.org/wiki/Monitor_analyze_plan_execute) and related control-loop architectures.

It provides:
- A **kernel** that orchestrates agent execution.
- A pluggable **middleware pipeline**.
- Extensible **loop profiles** (e.g. MAPE-K, reactive loops).
- Minimal abstractions to keep agents composable and testable.

---

## 📦 Installation

From NuGet:

```bash
dotnet add package AgentFramework.Kernel
```

---

## 🚀 Quick Start

Create a kernel, a profile, and run a tick:

```csharp
using AgentFramework.Kernel;
using AgentFramework.Kernel.Abstractions;

var profile = new TestProfile(); // sample profile (see HelloKernel)
var kernel = new AgentKernel(profile, new IKernelMiddleware[]
{
    new LoggingMiddleware()
});

await kernel.StartAsync();
await kernel.TickAsync();
await kernel.StopAsync();
```

👉 See the [HelloKernel sample](samples/HelloKernel) for a runnable console app.

---

## 🧩 Key Concepts

### Kernel
The **`AgentKernel`** is the control loop runner.  
It starts, stops, and executes ticks, invoking each step defined by the active profile.

### Profiles
A **`ILoopProfile`** defines the ordered steps an agent executes.  
Examples:
- **MAPE-K** (Monitor → Analyze → Plan → Execute → Knowledge update).
- **Reactive** loop (Sense → Act).

Profiles are the extensibility point for experimenting with different agent families.

### Middleware
Middleware implements `IKernelMiddleware` and runs **before/after each step**:
- Tracing
- Logging
- Metrics
- Policy enforcement

### Context
`KernelContext` flows through the pipeline and provides:
- The current `StepId`
- Cancellation token
- A scratchpad (`Items`) for cross-cutting state

---

## 🧪 Samples & Tests

- **[samples/HelloKernel](samples/HelloKernel)** – minimal console app showing a test profile and logging middleware.
- **[tests/AgentFramework.Kernel.Tests](tests/AgentFramework.Kernel.Tests)** – xUnit smoke tests for kernel contracts.

Run tests:

```bash
make test
```

Run the sample:

```bash
make hello-kernel
```

---

## 🔄 Versioning & Releases

- Versions are managed by [semantic-release](https://semantic-release.gitbook.io/) and git tags (`vX.Y.Z`).
- Pre-1.0: **breaking changes bump minor**, not major (`preMajor:true`).
- Package versions are injected at build time via [MinVer](https://github.com/adamralph/minver).
- See [CHANGELOG.md](CHANGELOG.md) for release history.

Dry-run a release locally:

```bash
make release-dry-run
```

---

## 🤝 Contributing

1. Fork & clone  
2. Run `make build` / `make test`  
3. Add features with Conventional Commit messages (`feat: ...`, `fix: ...`)  
4. Open a PR

---

## 📄 License

MIT © [David Runemalm](https://github.com/runemalm)
