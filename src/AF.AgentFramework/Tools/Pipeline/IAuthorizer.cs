using AgentFramework.Tools.Contracts;
using AgentFramework.Tools.Registry;

namespace AgentFramework.Tools.Pipeline;

/// <summary>
/// Decides whether a given invocation is authorized for execution.
/// Default implementation may always allow; hosts can replace with policy-aware authorizers.
/// </summary>
public interface IAuthorizer
{
    AuthorizationOutcome Authorize(ToolInvocation invocation, ToolDescriptor descriptor, ToolBindingContext bindingContext);
}

/// <summary>
/// Result of authorization.
/// </summary>
public sealed record class AuthorizationOutcome
{
    public bool IsAuthorized { get; init; }
    public string? Reason { get; init; }

    public static AuthorizationOutcome Allow() => new() { IsAuthorized = true };
    public static AuthorizationOutcome Deny(string reason) => new() { IsAuthorized = false, Reason = reason };
}