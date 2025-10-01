namespace AgentFramework.Tools.Contracts;

/// <summary>
/// High-level error categories returned from tool invocations.
/// </summary>
public enum ToolErrorCode
{
    /// <summary>
    /// Contract/schema/version issues detected before or after execution.
    /// </summary>
    ContractError = 0,

    /// <summary>
    /// A reliability/safety policy prevented successful execution
    /// (e.g., timeout, circuit open, rate limited, budget exceeded).
    /// </summary>
    PolicyError = 1,

    /// <summary>
    /// Authentication or authorization failure.
    /// </summary>
    AuthError = 2,

    /// <summary>
    /// Failure in the underlying provider or downstream service during execution.
    /// </summary>
    ExecutionError = 3,

    /// <summary>
    /// Unexpected runtime failure not covered by other categories.
    /// </summary>
    SystemError = 4
}
