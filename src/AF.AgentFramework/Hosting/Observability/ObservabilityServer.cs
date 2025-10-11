using System.Text;
using AgentFramework.Kernel.Diagnostics;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace AgentFramework.Hosting.Observability;

/// <summary>
/// Minimal embedded observability HTTP server exposing live kernel snapshots.
/// </summary>
public sealed class ObservabilityServer : IAsyncDisposable
{
    private readonly int _port;
    private readonly IKernelInspector _inspector;

    private IHost? _host;
    private Task? _runTask;

    public ObservabilityServer(int port, IKernelInspector inspector)
    {
        _port = port;
        _inspector = inspector ?? throw new ArgumentNullException(nameof(inspector));
    }

    public async Task StartAsync(CancellationToken ct = default)
    {
        if (_host is not null)
            throw new InvalidOperationException("Observability server already started.");

        Console.WriteLine($"[Observability] Starting on http://localhost:{_port}/af …");

        var hostBuilder = new HostBuilder()
            .ConfigureWebHost(webBuilder =>
            {
                webBuilder
                    .UseKestrel()
                    .UseUrls($"http://localhost:{_port}")
                    .ConfigureServices(services =>
                    {
                        services.AddRouting();
                    })
                    .Configure(app =>
                    {
                        var routeBuilder = app.New();
                        app.UseRouting();

                        app.UseEndpoints(endpoints =>
                        {
                            endpoints.MapGet("/af/snapshot", async context =>
                            {
                                try
                                {
                                    var snapshot = _inspector.GetSnapshot();
                                    await context.Response.WriteAsJsonAsync(snapshot);
                                }
                                catch (Exception ex)
                                {
                                    context.Response.StatusCode = 500;
                                    await context.Response.WriteAsync($"Snapshot failed: {ex.Message}");
                                }
                            });

                            endpoints.MapGet("/af", async context =>
                            {
                                context.Response.ContentType = "text/html; charset=utf-8";
                                var html = await GetDashboardHtmlAsync();
                                await context.Response.WriteAsync(html, Encoding.UTF8);
                            });

                            endpoints.MapGet("/af/health", async ctx =>
                                await ctx.Response.WriteAsync("OK"));
                        });
                    });
            })
            .UseConsoleLifetime(opts => opts.SuppressStatusMessages = true);

        _host = hostBuilder.Build();

        _runTask = _host.RunAsync(ct);

        Console.WriteLine("[Observability] Started.");
        Console.WriteLine($"[Host] Dashboard available at http://localhost:{_port}/af");
    }

    public async Task StopAsync(CancellationToken ct = default)
    {
        if (_host is null) return;

        Console.WriteLine("[Observability] Stopping …");

        await _host.StopAsync(ct);

        if (_runTask is not null)
            await _runTask.ConfigureAwait(false);

        _host.Dispose();
        _host = null;

        Console.WriteLine("[Observability] Stopped.");
    }


    public async ValueTask DisposeAsync()
    {
        if (_host is not null)
            await StopAsync();
    }

    private static async Task<string> GetDashboardHtmlAsync()
    {
        var asm = typeof(ObservabilityServer).Assembly;
        using var stream = asm.GetManifestResourceStream("AgentFramework.Hosting.Observability.Dashboard.html")!;
        using var reader = new StreamReader(stream, Encoding.UTF8);
        return await reader.ReadToEndAsync();
    }
}
