using System.Text;
using AgentFramework.Kernel.Diagnostics;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AgentFramework.Hosting.Observability;

/// <summary>
/// Lightweight embedded observability web server exposing
/// live kernel snapshots and a dashboard UI.
/// Runs inside the same process and DI scope as the main AgentHost.
/// </summary>
public sealed class ObservabilityServer : IAsyncDisposable
{
    private readonly int _port;
    private readonly IKernelInspector _inspector;
    private readonly ILogger<ObservabilityServer> _logger;
    private IWebHost? _webHost;

    public ObservabilityServer(int port, IKernelInspector inspector, ILogger<ObservabilityServer> logger)
    {
        _port = port;
        _inspector = inspector ?? throw new ArgumentNullException(nameof(inspector));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Starts the embedded dashboard HTTP server.
    /// </summary>
    public async Task StartAsync(CancellationToken ct = default)
    {
        if (_webHost is not null)
            throw new InvalidOperationException("Observability server already started.");

        _logger.LogInformation("Starting observability server on http://localhost:{port}/af ...", _port);

        _webHost = new WebHostBuilder()
            .UseKestrel()
            .UseUrls($"http://localhost:{_port}")
            .ConfigureServices(services =>
            {
                services.AddRouting();
            })
            .Configure(app =>
            {
                app.UseRouting();
                app.UseEndpoints(endpoints =>
                {
                    endpoints.MapGet("/af/health", async ctx =>
                    {
                        await ctx.Response.WriteAsync("OK");
                    });

                    endpoints.MapGet("/af/snapshot", async ctx =>
                    {
                        try
                        {
                            var snapshot = _inspector.GetSnapshot();
                            await ctx.Response.WriteAsJsonAsync(snapshot);
                        }
                        catch (Exception ex)
                        {
                            ctx.Response.StatusCode = 500;
                            await ctx.Response.WriteAsync($"Snapshot failed: {ex.Message}");
                        }
                    });

                    endpoints.MapGet("/af", async ctx =>
                    {
                        ctx.Response.ContentType = "text/html; charset=utf-8";
                        var html = await GetDashboardHtmlAsync();
                        await ctx.Response.WriteAsync(html, Encoding.UTF8);
                    });
                });
            })
            .Build();

        await _webHost.StartAsync(ct);

        _logger.LogInformation("Observability dashboard available at http://localhost:{port}/af", _port);
    }

    /// <summary>
    /// Stops the web server gracefully.
    /// </summary>
    public async Task StopAsync(CancellationToken ct = default)
    {
        if (_webHost is null)
            return;

        _logger.LogInformation("Stopping observability server...");
        await _webHost.StopAsync(ct);
        _webHost.Dispose();
        _webHost = null;
        _logger.LogInformation("Observability server stopped.");
    }

    public async ValueTask DisposeAsync()
    {
        if (_webHost is not null)
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
