namespace AgentFramework.Agent.Policies;

/// <summary>
/// Routes DecideAsync to a sub-policy based on a classifier function.
/// If no route matches, falls back to a default policy (if provided) or returns a neutral reply.
/// </summary>
public sealed class RouterPolicy : IPolicy
{
    private readonly IReadOnlyDictionary<string, IPolicy> _routes;
    private readonly Func<AgentContext, string> _classify;
    private readonly IPolicy? _fallback;

    public RouterPolicy(
        IReadOnlyDictionary<string, IPolicy> routes,
        Func<AgentContext, string> classify,
        IPolicy? fallback = null)
    {
        _routes = routes ?? throw new ArgumentNullException(nameof(routes));
        _classify = classify ?? throw new ArgumentNullException(nameof(classify));
        _fallback = fallback;
    }

    public Task<Decision> DecideAsync(AgentContext context, CancellationToken ct = default)
    {
        var key = _classify(context) ?? string.Empty;

        if (_routes.TryGetValue(key, out var policy))
            return policy.DecideAsync(context, ct);

        if (_fallback is not null)
            return _fallback.DecideAsync(context, ct);

        var msg = string.IsNullOrWhiteSpace(key)
            ? "I couldn't determine what to do."
            : $"No route for intent '{key}'.";
        return Task.FromResult(new Decision(new Message("assistant", msg), Array.Empty<ToolCall>()));
    }
}
