# AgentFramework.Agent

[![NuGet version](https://img.shields.io/nuget/v/AgentFramework.Agent.svg)](https://www.nuget.org/packages/AgentFramework.Agent/)  
[![Build status](https://github.com/runemalm/AgentFramework.Agent/actions/workflows/release.yml/badge.svg?branch=master)](https://github.com/runemalm/AgentFramework.Agent/actions/workflows/release.yml)  
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

An agent framework for .NET.

## Install

```bash
dotnet add package AgentFramework.Agent
```

## Quick start

```csharp
using AgentFramework.Agent;

var (agent, ctx) = new AgentBuilder()
    .UsePolicy(new EchoPolicy()) // replace with your own policy
    .Build(new AgentId("assistant-1"));

ctx.Conversation.Add(new Message("user", "Hello Agent"));
var reply = await agent.StepAsync(ctx);

Console.WriteLine(reply?.Content); // "Hello Agent"
```

## Concepts

- `Agent` - orchestrates **Decide → Act → Learn**
- `IPolicy` - reasoning strategy (LLM, rules, heuristics)
- `ITool` / `IToolRegistry` - executable capabilities
- `IMemoryStore` - episodic/semantic memory
- `IAgentMiddleware` - cross-cutting concerns
- `AgentBuilder` - composition helper for building agents

## Philosophy

- **Open for extension, closed for modification**  
  Add new tools, policies, memory stores, or middlewares without touching the core.
- **Minimal agent**  
  Just the agent loop and a handful of stable abstractions.
- **Composable**  
  Everything else is pluggable: swap in your own LLM policy, vector memory, or tracing middleware.

## Example: add a tool

```csharp
public sealed class WeatherTool : ITool
{
    public string Name => "weather.get";
    public string Description => "Get current weather by city name";

    public Task<ToolResult> InvokeAsync(ToolContext context, string argsJson, CancellationToken ct = default)
    {
        var city = System.Text.Json.JsonDocument.Parse(argsJson).RootElement.GetProperty("city").GetString();
        var result = new ToolResult(Name, $"{"city":"{city}","tempC":21}");
        return Task.FromResult(result);
    }
}
```

Register the tool:

```csharp
var (agent, ctx) = new AgentBuilder()
    .UsePolicy(new EchoPolicy())
    .AddTool(new WeatherTool())
    .Build(new AgentId("assistant-1"));
```

## Status

- Early preview, public API subject to change before `1.0.0`.
- Targeting .NET 8 (LTS).
- MIT licensed.

## License

MIT
