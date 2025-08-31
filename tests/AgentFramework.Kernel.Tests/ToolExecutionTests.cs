using System.Text.Json;

namespace AgentFramework.Kernel.Tests;

public class ToolExecutionTests
{
    private sealed class RecordingMemory : IMemoryStore
    {
        public readonly List<MemoryEntry> Entries = new();
        public Task AppendAsync(MemoryEntry entry, CancellationToken ct = default)
        { Entries.Add(entry); return Task.CompletedTask; }
        public async IAsyncEnumerable<MemoryEntry> QueryAsync(MemoryQuery query, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct = default)
        { await Task.CompletedTask; yield break; }
    }

    private sealed class FakeTool(string name) : ITool
    {
        public string Name => name;
        public string Description => "test";
        public Task<ToolResult> InvokeAsync(ToolContext context, string argsJson, CancellationToken ct = default)
            => Task.FromResult(new ToolResult(Name, JsonSerializer.Serialize(new { ok = true, args = argsJson })));
    }

    private sealed class EmitTwoToolsPolicy : IPolicy
    {
        public Task<Decision> DecideAsync(AgentContext ctx, CancellationToken ct = default)
        {
            var calls = new[]
            {
                new ToolCall("t1", """{"a":1}"""),
                new ToolCall("t2", """{"b":2}""")
            };
            return Task.FromResult(new Decision(new Message("assistant", "done"), calls));
        }
    }

    [Fact]
    public async Task Executes_Tools_Appends_Tool_Messages_Then_Reply_And_Writes_Memory()
    {
        var mem = new RecordingMemory();

        var builder = new AgentBuilder()
            .UsePolicy(new EmitTwoToolsPolicy())
            .UseMemory(mem)
            .AddTool(new FakeTool("t1"))
            .AddTool(new FakeTool("t2"));

        var (agent, ctx) = builder.Build(new AgentId("t"));
        ctx.Conversation.Add(new Message("user", "run"));

        var reply = await agent.StepAsync(ctx);

        // tool messages
        var toolMsgs = ctx.Conversation.Where(m => m.Role == "tool").ToList();
        Assert.Equal(2, toolMsgs.Count);
        Assert.StartsWith("t1:", toolMsgs[0].Content);
        Assert.StartsWith("t2:", toolMsgs[1].Content);

        // reply last
        var last = ctx.Conversation.Last();
        Assert.Equal("assistant", last.Role);
        Assert.Equal("done", last.Content);

        // memory entry with toolCalls count == 2
        var stepEntry = mem.Entries.LastOrDefault(e => e.Kind == "step");
        Assert.NotNull(stepEntry);
        Assert.Contains("\"toolCalls\":2", stepEntry!.Json);
    }

    private sealed class MissingToolPolicy : IPolicy
    {
        public Task<Decision> DecideAsync(AgentContext ctx, CancellationToken ct = default)
            => Task.FromResult(new Decision(new Message("assistant", "nope"), new[] { new ToolCall("missing", "{}") }));
    }

    [Fact]
    public async Task Throws_If_Tool_Missing()
    {
        var builder = new AgentBuilder()
            .UsePolicy(new MissingToolPolicy()); // no tools added

        var (agent, ctx) = builder.Build(new AgentId("t"));
        ctx.Conversation.Add(new Message("user", "run"));

        await Assert.ThrowsAsync<InvalidOperationException>(() => agent.StepAsync(ctx));
    }
}
