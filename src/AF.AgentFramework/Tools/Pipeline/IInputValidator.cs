namespace AgentFramework.Tools.Pipeline;

/// <summary>
/// Validates invocation input against the tool's declared input schema.
/// Default implementation may be a pass-through; hosts can replace with a real validator.
/// </summary>
public interface IInputValidator
{
    ValidationOutcome Validate(object? input, object? inputSchema);
}

/// <summary>
/// Result of input validation.
/// </summary>
public sealed record class ValidationOutcome
{
    public bool IsValid { get; init; }
    public string[] Errors { get; init; } = System.Array.Empty<string>();

    public static ValidationOutcome Valid() => new() { IsValid = true };
    public static ValidationOutcome Invalid(params string[] errors) => new() { IsValid = false, Errors = errors ?? System.Array.Empty<string>() };
}