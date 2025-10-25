using Microsoft.Extensions.Hosting;

namespace AgentFramework.Hosting;

/// <summary>
/// Lightweight wrapper around a .NET <see cref="IHost"/> representing an AgentFramework application.
/// Provides familiar entrypoints (<see cref="RunAsync"/> / <see cref="StopAsync"/>)
/// and access to the dependency injection container.
/// </summary>
public sealed class AgentHost : IAgentHost
{
    private readonly IHost _innerHost;

    internal AgentHost(IHost innerHost)
    {
        _innerHost = innerHost ?? throw new ArgumentNullException(nameof(innerHost));
    }

    /// <summary>Access to the underlying .NET service provider.</summary>
    public IServiceProvider Services => _innerHost.Services;

    /// <summary>
    /// Runs the agent host until cancellation is requested (e.g., Ctrl+C).
    /// Equivalent to <see cref="IHost.RunAsync"/>.
    /// </summary>
    public Task RunAsync(CancellationToken cancellationToken = default)
        => _innerHost.RunAsync(cancellationToken);

    /// <summary>
    /// Starts the host without blocking (advanced usage).
    /// Typically you’ll just call <see cref="RunAsync"/>.
    /// </summary>
    public Task StartAsync(CancellationToken cancellationToken = default)
        => _innerHost.StartAsync(cancellationToken);

    /// <summary>
    /// Gracefully stops the host.
    /// </summary>
    public Task StopAsync(CancellationToken cancellationToken = default)
        => _innerHost.StopAsync(cancellationToken);

    /// <summary>
    /// Disposes the underlying host asynchronously.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        await _innerHost.StopAsync();
        _innerHost.Dispose();
    }
}
