using AgentFramework.Kernel;
using AgentFramework.Kernel.Knowledge;
using AgentFramework.Kernel.Policies;
using AgentFramework.Kernel.Policies.Defaults;
using Xunit;

namespace AgentFramework.Tests.Kernel;

public class InProcKernelTests
{
    private static InProcKernel CreateKernel(IAgent agent, PolicySet? defaults = null)
    {
        var contextFactory = new TestAgentContextFactory();

        var opts = new KernelOptions
        {
            Agents = new TestAgentCatalog(agent),
            Defaults = defaults ?? PolicySetDefaults.Create(),
            Bindings = Array.Empty<AttachmentBinding>(),
            ContextFactory = contextFactory,
            WorkerCount = 1
        };
        return new InProcKernel(opts);
    }

    private static WorkItem WI(string agentId, string engineId, WorkItemKind kind, int priority = 0)
        => new WorkItem
        {
            Id = Guid.NewGuid().ToString("n"),
            AgentId = agentId,
            EngineId = engineId,
            Kind = kind,
            Priority = priority
        };

    [Fact]
    public async Task SingleActive_PerAgent()
    {
        var agent = new TestAgent("a", workDuration: TimeSpan.FromMilliseconds(150));
        using var kernel = CreateKernel(agent);
        await kernel.StartAsync();

        await kernel.EnqueueAsync(WI("a", "loop", WorkItemKind.Job));
        await kernel.EnqueueAsync(WI("a", "loop", WorkItemKind.Job));

        await Task.Delay(500);
        await kernel.StopAsync();

        Assert.Equal(2, agent.Starts.Count);
        // second start should be after first finish (no overlap)
        Assert.True(agent.Starts[1].Started >= agent.Finishes[0].Finished);
    }

    [Fact]
    public async Task Ordering_ByPriority()
    {
        var agent = new TestAgent("a", workDuration: TimeSpan.FromMilliseconds(50));
        using var kernel = CreateKernel(agent);
        await kernel.StartAsync();

        var low = WI("a", "loop", WorkItemKind.Job, priority: 0);
        var high = WI("a", "loop", WorkItemKind.Job, priority: 10);

        // enqueue low then high; high should run first
        await kernel.EnqueueAsync(low);
        await kernel.EnqueueAsync(high);

        await Task.Delay(400);
        await kernel.StopAsync();

        Assert.Equal(2, agent.HandledOrder.Count);
        Assert.Equal(high.Id, agent.HandledOrder[0]);
        Assert.Equal(low.Id, agent.HandledOrder[1]);
    }

    [Fact]
    public async Task Timeout_Cancels_And_NoRetry()
    {
        var agent = new TestAgent("a", workDuration: TimeSpan.FromMilliseconds(500));
        var defaults = PolicySetDefaults.Create(timeout: new TimeoutOptions(TimeSpan.FromMilliseconds(100)));
        using var kernel = CreateKernel(agent, defaults);
        await kernel.StartAsync();

        await kernel.EnqueueAsync(WI("a", "loop", WorkItemKind.Job));
        await Task.Delay(500);
        await kernel.StopAsync();

        // one attempt that was canceled; no retries
        Assert.Single(agent.Canceled);
        Assert.True(agent.Starts.Count == 1);
        Assert.True(agent.HandledOrder.Count == 0);
    }

    [Fact]
    public async Task Retry_OnFailure_With_Backoff()
    {
        // Fail first attempt, then succeed
        var agent = new TestAgent("a", workDuration: TimeSpan.FromMilliseconds(50), failAttempts: 1);
        var defaults = PolicySetDefaults.Create(
            retry: RetryOptions.Default with { MaxAttempts = 2, BaseDelay = TimeSpan.FromMilliseconds(50) });

        using var kernel = CreateKernel(agent, defaults);
        await kernel.StartAsync();

        await kernel.EnqueueAsync(WI("a", "loop", WorkItemKind.Job));
        await Task.Delay(600);
        await kernel.StopAsync();

        // Should have 1 success in HandledOrder (second attempt), and two starts total
        Assert.True(agent.Starts.Count >= 2, "expected at least two attempts");
        Assert.Single(agent.HandledOrder);
    }

    private sealed class AlwaysCooperatePreemption : IPreemptionPolicy
    {
        public PreemptionDecision ShouldPreempt(WorkItem incoming, RunningInvocation current)
            => PreemptionDecision.Cooperative;
    }

    [Fact]
    public async Task Cooperative_Preemption_Cancels_Current_And_Runs_Incoming()
    {
        // Long-running first item; then enqueue high-priority incoming that should preempt
        var agent = new TestAgent("a", workDuration: TimeSpan.FromMilliseconds(500));
        var defaults = PolicySetDefaults.Create(preemption: new AlwaysCooperatePreemption());

        using var kernel = CreateKernel(agent, defaults);
        await kernel.StartAsync();

        var longItem = WI("a", "loop", WorkItemKind.Job, priority: 0);
        await kernel.EnqueueAsync(longItem);

        await Task.Delay(200); // let it start
        var urgent = WI("a", "loop", WorkItemKind.Job, priority: 100);
        await kernel.EnqueueAsync(urgent);

        await Task.Delay(1200);
        await kernel.StopAsync();

        // Long item should be canceled; urgent should run
        Assert.Contains(longItem.Id, agent.Canceled);
        Assert.Contains(urgent.Id, agent.HandledOrder);
    }

    [Fact]
    public async Task Backpressure_Shed_Drops_Enqueue()
    {
        var agent = new TestAgent("a", workDuration: TimeSpan.FromMilliseconds(50));
        var defaults = PolicySetDefaults.Create(
            backpressure: new BackpressureOptions(ThrottleThreshold: 0, ShedThreshold: 0));

        using var kernel = CreateKernel(agent, defaults);
        await kernel.StartAsync();

        await kernel.EnqueueAsync(WI("a", "loop", WorkItemKind.Job));
        await Task.Delay(250);
        await kernel.StopAsync();

        // Shed means the item never ran
        Assert.Empty(agent.HandledOrder);
        Assert.Empty(agent.Starts);
    }
    
    // --- Helper factory ---
    private sealed class TestAgentContextFactory : IAgentContextFactory
    {
        public IAgentContext CreateContext(WorkItem item, CancellationToken cancellation)
        {
            return new AgentContext(
                item.AgentId,
                item.EngineId,
                item.Id,
                item.CorrelationId,
                cancellation,
                randomSeed: item.Id.GetHashCode(),
                knowledge: new InMemoryKnowledge(),
                tools: null
            );
        }
    }
}
