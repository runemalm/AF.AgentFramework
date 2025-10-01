namespace AgentFramework.Tools.Pipeline.Defaults;

/// <summary>
/// Accepts any input without schema validation. Replace in host for real validation.
/// </summary>
public sealed class PassThroughInputValidator : IInputValidator
{
    public ValidationOutcome Validate(object? input, object? inputSchema) => ValidationOutcome.Valid();
}