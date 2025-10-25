namespace AgentFramework.Hosting;

/// <summary>
/// Represents a running AgentFramework host application.
/// This is a lightweight abstraction over the underlying .NET <see cref="Microsoft.Extensions.Hosting.IHost"/>.
/// </summary>
public interface IAgentHost : IAsyncDisposable
{
    /// <summary>
    /// Access to the service provider for advanced scenarios.
    /// </summary>
    IServiceProvider Services { get; }

    /// <summary>
    /// Starts the host without blocking. Typically you’ll use <see cref="RunAsync"/> instead.
    /// </summary>
    Task StartAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Runs the host until shutdown is requested (e.g., Ctrl+C or SIGTERM).
    /// </summary>
    Task RunAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gracefully stops the host and all hosted services.
    /// </summary>
    Task StopAsync(CancellationToken cancellationToken = default);
}
