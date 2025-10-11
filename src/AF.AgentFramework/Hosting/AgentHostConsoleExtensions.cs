namespace AgentFramework.Hosting;

public static class AgentHostConsoleExtensions
{
    /// <summary>
    /// Runs the AgentHost until Ctrl+C is pressed, handling graceful shutdown and fatal errors.
    /// </summary>
    public static async Task RunConsoleAsync(this IAgentHost host)
    {
        if (host is null) throw new ArgumentNullException(nameof(host));

        using var cts = new CancellationTokenSource();

        Console.CancelKeyPress += (s, e) =>
        {
            e.Cancel = true;
            Console.WriteLine("[Host] Ctrl+C received → initiating shutdown…");
            cts.Cancel();
        };

        AppDomain.CurrentDomain.UnhandledException += (_, exArgs) =>
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("[Host] FATAL: Unhandled exception");
            Console.ResetColor();
            Console.WriteLine(exArgs.ExceptionObject.ToString());
            cts.Cancel();
        };

        TaskScheduler.UnobservedTaskException += (_, exArgs) =>
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("[Host] FATAL: Unobserved task exception");
            Console.ResetColor();
            Console.WriteLine(exArgs.Exception.ToString());
            exArgs.SetObserved();
            cts.Cancel();
        };

        try
        {
            await host.StartAsync(cts.Token);
            Console.WriteLine("[Host] Running (press Ctrl+C to exit)…");

            // Keep process alive until canceled
            await Task.Delay(Timeout.Infinite, cts.Token);
        }
        catch (TaskCanceledException)
        {
            // Expected on Ctrl+C or fatal error
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("[Host] FATAL: Exception during runtime");
            Console.ResetColor();
            Console.WriteLine(ex.ToString());
        }
        finally
        {
            try
            {
                await host.StopAsync(cts.Token);
                Console.WriteLine("[Host] Shutdown complete.");
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[Host] WARNING: Exception during shutdown");
                Console.ResetColor();
                Console.WriteLine(ex.ToString());
            }
        }
    }
}
