namespace AgentFramework.Tools.Pipeline.Defaults;

/// <summary>
/// Provides singleton instances of the default pipeline components.
/// Useful for tests or quick bootstrap without DI.
/// </summary>
public static class Defaults
{
    public static readonly PassThroughInputValidator InputValidator = new();
    public static readonly AllowAllAuthorizer Authorizer = new();
    public static readonly TimeoutOnlyPolicyApplier PolicyApplier = new();
    public static readonly NotImplementedExecutor Executor = new();
    public static readonly PassThroughPostprocessor Postprocessor = new();
}