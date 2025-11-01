using AgentFramework.Runners;

namespace HelloKernel.Runners;

/// <summary>
/// Mock runner that emits simulated Slack events to demonstrate reactive perception.
/// </summary>
sealed class SlackMockRunner : IReactiveRunner
{
    public string Name { get; } = "SlackMockRunner";

    public Func<object?, string?, CancellationToken, Task>? OnEventAsync { get; set; }

    private readonly string[] _topics = ["slack/message", "slack/reaction"];
    private readonly string[] _users = ["alice", "bob", "charlie"];
    private readonly Random _rand = new();

    public Task StartAsync(CancellationToken ct)
    {
        _ = Task.Run(async () =>
        {
            while (!ct.IsCancellationRequested)
            {
                var topic = _topics[_rand.Next(_topics.Length)];
                var user = _users[_rand.Next(_users.Length)];
                var ev = new SlackEvent(topic, user, $"Hello from {user}!");
                await OnEventAsync?.Invoke(ev, ev.Topic, ct)!;
                await Task.Delay(2000, ct);
            }
        }, ct);

        Console.WriteLine("[Runner] SlackMockRunner started (simulating Slack events).");
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken ct)
    {
        Console.WriteLine("[Runner] SlackMockRunner stopped.");
        return Task.CompletedTask;
    }
}

/// <summary>
/// Simplified Slack event payload.
/// </summary>
public sealed record SlackEvent(string Topic, string User, string Text);
