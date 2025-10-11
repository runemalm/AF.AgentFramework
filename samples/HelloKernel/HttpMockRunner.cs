using AgentFramework.Runners;

namespace HelloKernel;

sealed class HttpMockRunner : IReactiveRunner
{
    public string Name { get; } = "HttpMockRunner";

    public Func<object?, string?, CancellationToken, Task>? OnEventAsync { get; set; }

    private CancellationTokenSource? _cts;

    public Task StartAsync(CancellationToken ct = default)
    {
        _cts = CancellationTokenSource.CreateLinkedTokenSource(ct);

        _ = Task.Run(async () =>
        {
            int counter = 0;
            while (!_cts.Token.IsCancellationRequested)
            {
                counter++;
                var payload = $"HTTP POST #{counter}";
                Console.WriteLine($"[Runner] ({Name}) Received external event: {payload}");

                if (OnEventAsync is not null)
                    await OnEventAsync(payload, "http", _cts.Token).ConfigureAwait(false);

                await Task.Delay(1500, _cts.Token);
            }
        }, _cts.Token);

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken ct = default)
    {
        _cts?.Cancel();
        Console.WriteLine($"[Runner] ({Name}) stopped.");
        return Task.CompletedTask;
    }
}