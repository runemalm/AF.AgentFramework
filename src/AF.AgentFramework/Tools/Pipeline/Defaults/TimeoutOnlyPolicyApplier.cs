using AgentFramework.Tools.Contracts;

namespace AgentFramework.Tools.Pipeline.Defaults;

/// <summary>
/// Applies only a timeout policy, derived from invocation.DeadlineUtc or the tool's default timeout.
/// Other policies (retries, rate limits, circuits, budgets) are ignored in this initial slice.
/// </summary>
public sealed class TimeoutOnlyPolicyApplier : IPolicyApplier
{
    public PolicyApplicationResult Apply(ToolInvocation invocation, ToolDescriptor descriptor, CancellationToken outerToken)
    {
        // Determine effective timeout (ms)
        int? timeoutMs = null;

        if (invocation.DeadlineUtc is { } deadline)
        {
            var now = DateTimeOffset.UtcNow;
            var remaining = deadline - now;
            if (remaining <= TimeSpan.Zero)
            {
                // Already past deadline; create a token that is already canceled
                var ctsImmediate = CancellationTokenSource.CreateLinkedTokenSource(outerToken);
                ctsImmediate.Cancel();
                return new PolicyApplicationResult
                {
                    Snapshot = new PolicySnapshot { TimeoutMs = 0, RetryEnabled = false },
                    EffectiveToken = ctsImmediate.Token
                };
            }
            timeoutMs = (int)Math.Ceiling(remaining.TotalMilliseconds);
        }
        else if (descriptor.Contract.DefaultPolicies.TimeoutMs is int fromContract)
        {
            timeoutMs = fromContract;
        }

        if (timeoutMs is int ms && ms > 0)
        {
            var cts = CancellationTokenSource.CreateLinkedTokenSource(outerToken);
            try
            {
                cts.CancelAfter(ms);
            }
            catch (ObjectDisposedException)
            {
                // Fallback if outer token is already disposed/canceled
                cts = new CancellationTokenSource(ms);
            }

            return new PolicyApplicationResult
            {
                Snapshot = new PolicySnapshot
                {
                    TimeoutMs = ms,
                    RetryEnabled = false
                },
                EffectiveToken = cts.Token
            };
        }

        // No timeout configured; just pass through the outer token
        return new PolicyApplicationResult
        {
            Snapshot = new PolicySnapshot
            {
                TimeoutMs = null,
                RetryEnabled = false
            },
            EffectiveToken = outerToken
        };
    }
}
