namespace AgentFramework.Runners;

/// <summary>
/// A runner that emits environment events to a reactive engine.
/// </summary>
public interface IReactiveRunner : IRunner
{
    /// <summary>
    /// Callback invoked by the runner when an external event occurs.
    /// </summary>
    Func<object?, string?, CancellationToken, Task>? OnEventAsync { get; set; }
}