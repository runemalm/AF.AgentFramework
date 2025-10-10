using System;
using System.Collections.Generic;
using System.Threading;

namespace AgentFramework.Kernel;

public interface IAgentContext
{
    string AgentId { get; }
    string EngineId { get; }
    string WorkItemId { get; }
    string? CorrelationId { get; }

    /// <summary>Cancellation for preemption, timeouts, and shutdown.</summary>
    CancellationToken Cancellation { get; }

    /// <summary>Deterministic time source (mockable).</summary>
    DateTimeOffset Now { get; }

    /// <summary>Scoped RNG for reproducible behavior in tests.</summary>
    Random Random { get; }

    /// <summary>Per-invocation scratchpad for stage handoffs.</summary>
    IDictionary<string, object?> Items { get; }

    /// <summary>Agent Knowledge store (the K in MAPE-K). Default is in-memory but pluggable.</summary>
    Knowledge.IKnowledge Knowledge { get; }

    /// <summary>Lightweight, correlated trace.</summary>
    void Trace(string message, IReadOnlyDictionary<string, object?>? data = null);
}