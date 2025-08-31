namespace AgentFramework.Kernel;

public interface IAgentMiddleware
{
    Task InvokeAsync(AgentContext context, Func<Task> next);
}

public sealed class Agent
{
    private readonly IPolicy _policy;
    private readonly IReadOnlyList<IAgentMiddleware> _middlewares;
    private readonly IMemoryStore _memory;

    public Agent(IPolicy policy, IEnumerable<IAgentMiddleware>? middlewares = null, IMemoryStore? memory = null)
    {
        _policy = policy;
        _middlewares = (middlewares ?? Array.Empty<IAgentMiddleware>()).ToList();
        _memory = memory ?? new NullMemoryStore();
    }

    public async Task<Message?> StepAsync(AgentContext context)
    {
        var i = -1;
        Task Next()
        {
            i++;
            return i < _middlewares.Count
                ? _middlewares[i].InvokeAsync(context, Next)
                : RunCoreAsync(context);
        }

        await Next();

        return context.Conversation.LastOrDefault(m => m.Role == "assistant");
    }

    private async Task RunCoreAsync(AgentContext ctx)
    {
        // Decide
        var decision = await _policy.DecideAsync(ctx, ctx.Cancellation);

        // Act (tools)
        foreach (var call in decision.ToolCalls)
        {
            var tool = ctx.Tools.Get(call.Name)
                ?? throw new InvalidOperationException($"Tool not found: {call.Name}");
            var result = await tool.InvokeAsync(new ToolContext(ctx), call.ArgumentsJson, ctx.Cancellation);
            ctx.Conversation.Add(new Message("tool", $"{result.Name}:{result.ResultJson}"));
        }

        // Learn
        await _memory.AppendAsync(new MemoryEntry(DateTimeOffset.UtcNow, "step",
            $"{{\"toolCalls\":{decision.ToolCalls.Count}}}"), ctx.Cancellation);

        if (decision.Reply is not null)
            ctx.Conversation.Add(decision.Reply);
    }

    // Minimal built-ins
    internal sealed class NullMemoryStore : IMemoryStore
    {
        public Task AppendAsync(MemoryEntry entry, CancellationToken ct = default) => Task.CompletedTask;
        public async IAsyncEnumerable<MemoryEntry> QueryAsync(MemoryQuery query, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct = default)
        { await Task.CompletedTask; yield break; }
    }
}
