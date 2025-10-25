namespace AgentFramework.Hosting;

/// <summary>
/// Represents kernel concurrency settings (worker count, etc.).
/// </summary>
public sealed record KernelConcurrency(int WorkerCount);
