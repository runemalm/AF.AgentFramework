namespace AgentFramework.Tools.Contracts;

/// <summary>
/// Outcome classification for a tool invocation.
/// </summary>
public enum ToolResultStatus
{
    /// <summary>
    /// Invocation completed successfully and produced an output.
    /// </summary>
    Ok = 0,

    /// <summary>
    /// Invocation failed and should not be retried by the pipeline.
    /// </summary>
    Error = 1,

    /// <summary>
    /// Invocation failed with a condition that may succeed on retry
    /// (subject to policy and effect semantics).
    /// </summary>
    Retryable = 2
}
