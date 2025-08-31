using System.Text.Json;
using System.Text.Json.Serialization;
using AgentFramework.Kernel;

namespace AgentFramework.Extras.Policies.Tests;

public class MapeKPolicyBaseTests
{
    // --- 1) Phase progression to "ok" --------------------------------------

    private sealed class SimpleFlowPolicy : MapeKPolicyBase
    {
        protected override Task<Decision> MonitorAsync(AgentContext ctx, WorkflowState s, CancellationToken ct)
        {
            // advance to Analyze
            SetState(ctx, s with { Phase = Phase.Analyze });
            return Task.FromResult(new Decision(new Message("assistant", "monitor"), Array.Empty<ToolCall>()));
        }

        protected override Task<Decision> AnalyzeAsync(AgentContext ctx, WorkflowState s, CancellationToken ct)
        {
            SetState(ctx, s with { Phase = Phase.Plan });
            return Task.FromResult(new Decision(new Message("assistant", "analyze"), Array.Empty<ToolCall>()));
        }

        protected override Task<Decision> PlanAsync(AgentContext ctx, WorkflowState s, CancellationToken ct)
        {
            SetState(ctx, s with { Phase = Phase.Execute, PlanJson = "{\"ok\":true}" });
            return Task.FromResult(new Decision(new Message("assistant", "plan"), Array.Empty<ToolCall>()));
        }

        protected override Task<Decision> ExecuteAsync(AgentContext ctx, WorkflowState s, CancellationToken ct)
        {
            SetState(ctx, s with { Phase = Phase.Done, Apply = true });
            return Task.FromResult(new Decision(new Message("assistant", "ok"), Array.Empty<ToolCall>()));
        }
    }

    [Fact]
    public async Task Runs_Through_Phases_To_Ok()
    {
        var (agent, ctx) = new AgentBuilder().UsePolicy(new SimpleFlowPolicy()).Build(new AgentId("t"));
        ctx.Conversation.Add(new Message("user", "go"));

        // Step until we see the terminal reply
        for (int i = 0; i < 8; i++)
        {
            var reply = await agent.StepAsync(ctx);
            if (reply?.Content == "ok") break;
        }

        Assert.Equal("ok", ctx.Conversation[^1].Content);
    }

    // --- 2) MaxSteps capping when no phase transition -----------------------

    private sealed class StuckMonitorPolicy : MapeKPolicyBase
    {
        protected override int MaxSteps => 3; // small cap for test

        protected override Task<Decision> MonitorAsync(AgentContext ctx, WorkflowState s, CancellationToken ct)
        {
            // Do not change phase; base should eventually return Done() when Step >= MaxSteps
            return Task.FromResult(new Decision(new Message("assistant", "tick"), Array.Empty<ToolCall>()));
        }

        protected override Task<Decision> AnalyzeAsync(AgentContext ctx, WorkflowState s, CancellationToken ct)
            => throw new NotSupportedException();

        protected override Task<Decision> PlanAsync(AgentContext ctx, WorkflowState s, CancellationToken ct)
            => throw new NotSupportedException();

        protected override Task<Decision> ExecuteAsync(AgentContext ctx, WorkflowState s, CancellationToken ct)
            => throw new NotSupportedException();
    }

    [Fact]
    public async Task Caps_By_MaxSteps_And_Returns_Done()
    {
        var (agent, ctx) = new AgentBuilder().UsePolicy(new StuckMonitorPolicy()).Build(new AgentId("t"));
        ctx.Conversation.Add(new Message("user", "go"));

        // First three steps: "tick"
        Assert.Equal("tick", (await agent.StepAsync(ctx))?.Content);
        Assert.Equal("tick", (await agent.StepAsync(ctx))?.Content);
        Assert.Equal("tick", (await agent.StepAsync(ctx))?.Content);

        // Next step: base detects cap and returns Done()
        Assert.Equal("Done.", (await agent.StepAsync(ctx))?.Content);
    }

    // --- 3) ReadToolJson / ReadTool<T> helper works with tool outputs -------

    private sealed class CalcAddTool : ITool
    {
        public string Name => "calc.add";
        public string Description => "Adds two integers";
        public Task<ToolResult> InvokeAsync(ToolContext context, string argumentsJson, CancellationToken ct = default)
        {
            var args = JsonSerializer.Deserialize<Args>(argumentsJson)!;
            var sum = args.a + args.b;
            return Task.FromResult(new ToolResult(Name, JsonSerializer.Serialize(new { sum })));
        }

        private sealed record Args(int a, int b);
    }

    private sealed class ToolReaderPolicy : MapeKPolicyBase
    {
        private sealed record SumDto([property: JsonPropertyName("sum")] int Sum);

        protected override Task<Decision> MonitorAsync(AgentContext ctx, WorkflowState s, CancellationToken ct)
        {
            // Request a tool call; next step (Analyze) will read its result
            SetState(ctx, s with { Phase = Phase.Analyze });
            var call = new ToolCall("calc.add", """{"a":1,"b":2}""");
            return Task.FromResult(new Decision(new Message("assistant", "calling tool"), new[] { call }));
        }

        protected override Task<Decision> AnalyzeAsync(AgentContext ctx, WorkflowState s, CancellationToken ct)
        {
            // Read the most recent tool result
            var dto = ReadTool<SumDto>(ctx, "calc.add");
            SetState(ctx, s with { Phase = Phase.Done });
            return Task.FromResult(new Decision(new Message("assistant", $"sum={dto?.Sum}"), Array.Empty<ToolCall>()));
        }

        protected override Task<Decision> PlanAsync(AgentContext ctx, WorkflowState s, CancellationToken ct)
            => throw new NotSupportedException();

        protected override Task<Decision> ExecuteAsync(AgentContext ctx, WorkflowState s, CancellationToken ct)
            => throw new NotSupportedException();
    }

    [Fact]
    public async Task ReadToolJson_And_ReadTool_Take_Last_Tool_Result()
    {
        var builder = new AgentBuilder()
            .UsePolicy(new ToolReaderPolicy())
            .AddTool(new CalcAddTool());

        var (agent, ctx) = builder.Build(new AgentId("t"));
        ctx.Conversation.Add(new Message("user", "sum please"));

        // Step 1: Monitor emits a tool call -> agent executes tool
        _ = await agent.StepAsync(ctx);

        // Step 2: Analyze reads tool output and replies
        var reply = await agent.StepAsync(ctx);

        Assert.Equal("sum=3", reply?.Content);
    }
}
