namespace AgentFramework.Kernel;

public readonly record struct AgentTickResult(
    bool Success,
    DateTimeOffset StartedAt,
    DateTimeOffset FinishedAt,
    Exception? Error = null
);