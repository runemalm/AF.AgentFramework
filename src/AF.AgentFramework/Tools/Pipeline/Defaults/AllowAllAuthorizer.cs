using AgentFramework.Tools.Contracts;
using AgentFramework.Tools.Registry;

namespace AgentFramework.Tools.Pipeline.Defaults;

/// <summary>
/// Authorizer that always allows every invocation.
/// Replace in host with real policy- or role-based authorization.
/// </summary>
public sealed class AllowAllAuthorizer : IAuthorizer
{
    public AuthorizationOutcome Authorize(ToolInvocation invocation, ToolDescriptor descriptor, ToolBindingContext bindingContext)
        => AuthorizationOutcome.Allow();
}