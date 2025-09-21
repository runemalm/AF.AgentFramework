namespace AgentFramework.Runners.Timers;

/// <summary>
/// Minimal periodic runner that invokes an async callback on a fixed interval.
/// </summary>
public sealed class TimerRunner : IRunner, IAsyncDisposable
{
    private readonly TimeSpan _period;
    private CancellationTokenSource? _cts;
    private Task? _loopTask;

    public string Name { get; }

    /// <summary>Engine sets this to receive ticks.</summary>
    public Func<CancellationToken, Task>? OnTickAsync { get; set; }

    public TimerRunner(TimeSpan period, string? name = null)
    {
        _period = period <= TimeSpan.Zero ? TimeSpan.FromSeconds(1) : period;
        Name = string.IsNullOrWhiteSpace(name) ? $"Timer({period})" : name!;
    }

    public Task StartAsync(CancellationToken ct = default)
    {
        if (_loopTask is not null) return Task.CompletedTask;

        _cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        var token = _cts.Token;

        Console.WriteLine($"[Runner] {Name} starting (period={_period}).");

        _loopTask = Task.Run(async () =>
        {
            try
            {
                using var timer = new PeriodicTimer(_period);
                while (await timer.WaitForNextTickAsync(token).ConfigureAwait(false))
                {
                    if (OnTickAsync is not null)
                    {
                        await OnTickAsync(token).ConfigureAwait(false);
                    }
                    else
                    {
                        Console.WriteLine($"[Runner] {Name} tick @ {DateTimeOffset.UtcNow:O}");
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // expected on stop
            }
        }, token);

        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken ct = default)
    {
        if (_cts is null) return;

        Console.WriteLine($"[Runner] {Name} stopping...");
        _cts.Cancel();

        try
        {
            if (_loopTask is not null)
                await _loopTask.ConfigureAwait(false);
        }
        catch (OperationCanceledException) { /* expected */ }
        finally
        {
            _loopTask = null;
            _cts.Dispose();
            _cts = null;
        }

        Console.WriteLine($"[Runner] {Name} stopped.");
    }

    public async ValueTask DisposeAsync()
    {
        await StopAsync().ConfigureAwait(false);
    }
}
