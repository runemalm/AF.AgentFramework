namespace AgentFramework.Tools.Contracts;

/// <summary>
/// Helper factory for common <see cref="ToolError"/> shapes used across the pipeline.
/// </summary>
public static class ToolErrors
{
    public static ToolError ToolNotFound(string toolName) => new()
    {
        Code = ToolErrorCode.ContractError,
        Subcode = "ToolNotFound",
        Message = $"Tool '{toolName}' not found.",
        IsRetryable = false,
        Origin = "unknown"
    };

    public static ToolError ValidationFailed(params string[] errors) => new()
    {
        Code = ToolErrorCode.ContractError,
        Subcode = "ValidationFailed",
        Message = "Input validation failed.",
        Details = errors,
        IsRetryable = false
    };

    public static ToolError NotAuthorized(string? reason = null) => new()
    {
        Code = ToolErrorCode.AuthError,
        Subcode = "NotAuthorized",
        Message = reason ?? "Authorization denied.",
        IsRetryable = false
    };

    public static ToolError Timeout() => new()
    {
        Code = ToolErrorCode.PolicyError,
        Subcode = "Timeout",
        Message = "Invocation exceeded deadline.",
        IsRetryable = true
    };

    public static ToolError NotImplemented(string toolName, string origin) => new()
    {
        Code = ToolErrorCode.ExecutionError,
        Subcode = "NotImplemented",
        Message = $"No executor/provider is bound for tool '{toolName}'.",
        IsRetryable = false,
        Origin = origin
    };

    public static ToolError Unhandled(string message, string details) => new()
    {
        Code = ToolErrorCode.SystemError,
        Subcode = "Unhandled",
        Message = message,
        Details = details,
        IsRetryable = false
    };
}