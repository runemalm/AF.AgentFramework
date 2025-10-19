namespace AgentFramework.Tools.Contracts;

/// <summary>
/// Helper factory for constructing <see cref="ToolResult"/> objects consistently.
/// </summary>
public static class ToolResultFactory
{
    public static ToolResult Success(string toolName, object? output, string correlationId, string version, string origin, int durationMs, PolicySnapshot snapshot) =>
        new()
        {
            Status = ToolResultStatus.Ok,
            Output = output,
            CorrelationId = correlationId,
            ToolName = toolName,
            ResolvedVersion = version,
            Origin = origin,
            DurationMs = durationMs,
            PolicySnapshot = snapshot,
            Attempts = 1,
            CompletedUtc = DateTimeOffset.UtcNow
        };

    public static ToolResult Failure(string toolName, ToolError error, string correlationId, string version, string origin, int durationMs, PolicySnapshot snapshot) =>
        new()
        {
            Status = error.IsRetryable ? ToolResultStatus.Retryable : ToolResultStatus.Error,
            Error = error,
            CorrelationId = correlationId,
            ToolName = toolName,
            ResolvedVersion = version,
            Origin = origin,
            DurationMs = durationMs,
            PolicySnapshot = snapshot,
            Attempts = 1,
            CompletedUtc = DateTimeOffset.UtcNow
        };
}