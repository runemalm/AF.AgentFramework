using System.Text.Json;
using AgentFramework.Kernel;

namespace AgentFramework.Extras.Policies;

/// <summary>
/// Base class for MAPE-K style policies (Monitor → Analyze → Plan → Execute).
/// Keeps minimal workflow state in AgentContext.Metadata and provides helpers
/// to read last user input and tool results from the conversation.
/// </summary>
public abstract class MapeKPolicyBase : IPolicy
{
    protected enum Phase { Monitor, Analyze, Plan, Execute, Done }

    /// <summary>Serialized working state kept in Metadata (the “K” in MAPE-K).</summary>
    protected sealed record WorkflowState(
        Phase Phase,
        string? Intent = null,
        string? ContextJson = null,
        string? PlanJson = null,
        bool Apply = false,
        int Step = 0,
        int Version = 1
    );

    protected virtual string StateKey => "agent.mape.state";
    protected virtual int MaxSteps => 16;

    public Task<Decision> DecideAsync(AgentContext ctx, CancellationToken ct = default)
    {
        var s = GetState(ctx) ?? new WorkflowState(Phase.Monitor);
        if (s.Phase == Phase.Done || s.Step >= MaxSteps)
        {
            SetState(ctx, s with { Phase = Phase.Done });
            return Task.FromResult(Done());
        }

        // advance the step counter for this decision
        s = s with { Step = s.Step + 1 };
        SetState(ctx, s);

        return s.Phase switch
        {
            Phase.Monitor => MonitorAsync(ctx, s, ct),
            Phase.Analyze => AnalyzeAsync(ctx, s, ct),
            Phase.Plan    => PlanAsync(ctx, s, ct),
            Phase.Execute => ExecuteAsync(ctx, s, ct),
            _             => Task.FromResult(Done())
        };
    }

    // ---- Abstract phase handlers ------------------------------------------

    protected abstract Task<Decision> MonitorAsync(AgentContext ctx, WorkflowState s, CancellationToken ct);
    protected abstract Task<Decision> AnalyzeAsync(AgentContext ctx, WorkflowState s, CancellationToken ct);
    protected abstract Task<Decision> PlanAsync(AgentContext ctx, WorkflowState s, CancellationToken ct);
    protected abstract Task<Decision> ExecuteAsync(AgentContext ctx, WorkflowState s, CancellationToken ct);

    // ---- Helpers for derived policies --------------------------------------

    protected WorkflowState? GetState(AgentContext ctx) => ctx.Metadata.Get<WorkflowState>(StateKey);
    protected void SetState(AgentContext ctx, WorkflowState s) => ctx.Metadata.Set(StateKey, s);

    /// <summary>Transition to the next phase; call this before returning from a phase.</summary>
    protected void Transition(AgentContext ctx, WorkflowState s, Phase next, Action<WorkflowState>? mutate = null)
    {
        var updated = s with { Phase = next };
        mutate?.Invoke(updated);
        SetState(ctx, updated);
    }

    protected static string LastUserText(AgentContext ctx)
        => ctx.Conversation.LastOrDefault(m => m.Role == "user")?.Content ?? string.Empty;

    /// <summary>Get the most recent tool JSON payload for a given tool name from the conversation.</summary>
    protected static string? ReadToolJson(AgentContext ctx, string toolName)
    {
        for (int i = ctx.Conversation.Count - 1; i >= 0; i--)
        {
            var m = ctx.Conversation[i];
            if (m.Role == "tool" && m.Content.StartsWith(toolName + ":", StringComparison.Ordinal))
                return m.Content[(toolName.Length + 1)..];
        }
        return null;
    }

    protected static T? ReadTool<T>(AgentContext ctx, string toolName)
        => ReadToolJson(ctx, toolName) is string json
            ? JsonSerializer.Deserialize<T>(json)
            : default;

    protected static Decision Done(string text = "Done.")
        => new(new Message("assistant", text), Array.Empty<ToolCall>());
}
