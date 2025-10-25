using AgentFramework.Hosting.Observability;
using AgentFramework.Kernel;
using AgentFramework.Kernel.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AgentFramework.Hosting.Services;

/// <summary>
/// Background service that manages the lifecycle of the embedded ObservabilityServer.
/// It runs within the main .NET Generic Host and exposes a dashboard for live kernel metrics.
/// </summary>
public sealed class ObservabilityDashboardService : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly int _port;
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILogger<ObservabilityDashboardService> _logger;
    private ObservabilityServer? _server;

    public ObservabilityDashboardService(
        IServiceProvider services,
        ILoggerFactory loggerFactory,
        ILogger<ObservabilityDashboardService> logger,
        DashboardPort port)
    {
        _services = services ?? throw new ArgumentNullException(nameof(services));
        _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _port = port.Value;
    }

    /// <summary>
    /// Executes the dashboard lifecycle while the host is running.
    /// </summary>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            // Try to locate a running kernel that implements IKernelInspector
            var inspector = _services.GetServices<IKernel>()
                .OfType<IKernelInspector>()
                .FirstOrDefault();

            if (inspector is null)
            {
                _logger.LogWarning("No IKernelInspector found; Observability Dashboard will not start.");
                return;
            }

            _logger.LogInformation("Starting Observability Dashboard on port {port}", _port);

            _server = new ObservabilityServer(
                _port,
                inspector,
                _loggerFactory.CreateLogger<ObservabilityServer>());

            await _server.StartAsync(stoppingToken);

            try
            {
                await Task.Delay(Timeout.Infinite, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                // expected on shutdown
            }

            _logger.LogInformation("Stopping Observability Dashboard...");
            await _server.StopAsync(stoppingToken);
            _logger.LogInformation("Observability Dashboard stopped.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start Observability Dashboard: {message}", ex.Message);
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_server is not null)
            await _server.StopAsync(cancellationToken);

        await base.StopAsync(cancellationToken);
    }
}
