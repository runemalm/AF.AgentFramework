using System.Collections.Concurrent;
using AgentFramework.Kernel;

namespace AgentFramework.Tests.Kernel;

internal sealed class TestAgent : IAgent
{
    private readonly TimeSpan _workDuration;
    private readonly int _failAttempts; // fail the first N attempts per WorkItem.Id
    private readonly ConcurrentDictionary<string, int> _attempts = new();

    public string Id { get; }

    public List<(string WorkItemId, DateTimeOffset Started)> Starts { get; } = new();
    public List<(string WorkItemId, DateTimeOffset Finished)> Finishes { get; } = new();
    public List<string> Canceled { get; } = new();
    public List<string> HandledOrder { get; } = new();

    public TestAgent(string id, TimeSpan? workDuration = null, int failAttempts = 0)
    {
        Id = id;
        _workDuration = workDuration ?? TimeSpan.FromMilliseconds(100);
        _failAttempts = failAttempts;
    }

    public async Task HandleAsync(WorkItem item, IAgentContext ctx)
    {
        Starts.Add((item.Id, DateTimeOffset.UtcNow));
        try
        {
            // Count attempt per WorkItem.Id, then fail until threshold is passed
            var attempt = _attempts.AddOrUpdate(item.Id, 1, (_, prev) => prev + 1);
            if (attempt <= _failAttempts)
            {
                throw new InvalidOperationException($"Simulated failure (attempt {attempt})");
            }

            // Simulate work honoring cancellation for timeout/preemption
            await Task.Delay(_workDuration, ctx.Cancellation);

            HandledOrder.Add(item.Id);
        }
        catch (OperationCanceledException)
        {
            Canceled.Add(item.Id);
            throw; // kernel shouldn't retry cancellations
        }
        finally
        {
            Finishes.Add((item.Id, DateTimeOffset.UtcNow));
        }
    }
}
