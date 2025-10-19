using System.Diagnostics;
using AgentFramework.Tools.Contracts;
using AgentFramework.Tools.Registry;

namespace AgentFramework.Tools.Pipeline;

/// <summary>
/// Default orchestrator that wires together registry resolution, validation,
/// authorization, policy enforcement, execution, and post-processing.
/// </summary>
public sealed class ToolPipeline : IToolInvoker
{
    private readonly IToolRegistry _registry;
    private readonly IInputValidator _validator;
    private readonly IAuthorizer _authorizer;
    private readonly IPolicyApplier _policyApplier;
    private readonly IExecutor _executor;
    private readonly IPostprocessor _postprocessor;

    public ToolPipeline(
        IToolRegistry registry,
        IInputValidator validator,
        IAuthorizer authorizer,
        IPolicyApplier policyApplier,
        IExecutor executor,
        IPostprocessor postprocessor)
    {
        _registry = registry;
        _validator = validator;
        _authorizer = authorizer;
        _policyApplier = policyApplier;
        _executor = executor;
        _postprocessor = postprocessor;
    }

    public async Task<ToolResult> InvokeAsync(ToolInvocation invocation, CancellationToken cancellationToken = default)
    {
        var sw = Stopwatch.StartNew();
        var correlationId = string.IsNullOrWhiteSpace(invocation.CorrelationId)
            ? Guid.NewGuid().ToString("N")
            : invocation.CorrelationId!;

        try
        {
            // 1. Resolve
            var descriptor = _registry.Resolve(invocation.ToolName, invocation.VersionRange, new ToolBindingContext { AgentId = "unknown" });
            if (descriptor is null)
            {
                return new ToolResult
                {
                    AgentId = invocation.AgentId,
                    Status = ToolResultStatus.Error,
                    Error = new ToolError
                    {
                        Code = ToolErrorCode.ContractError,
                        Subcode = "ToolNotFound",
                        Message = $"Tool '{invocation.ToolName}' not found.",
                        IsRetryable = false
                    },
                    CorrelationId = correlationId,
                    ToolName = invocation.ToolName,
                    DurationMs = (int)sw.ElapsedMilliseconds,
                    Origin = "unknown"
                };
            }

            // 2. Validate input
            var validation = _validator.Validate(invocation.Input, descriptor.Contract.InputSchema);
            if (!validation.IsValid)
            {
                return new ToolResult
                {
                    AgentId = invocation.AgentId,
                    Status = ToolResultStatus.Error,
                    Error = new ToolError
                    {
                        Code = ToolErrorCode.ContractError,
                        Subcode = "ValidationFailed",
                        Message = "Input validation failed.",
                        Details = validation.Errors,
                        IsRetryable = false
                    },
                    CorrelationId = correlationId,
                    ToolName = invocation.ToolName,
                    DurationMs = (int)sw.ElapsedMilliseconds,
                    Origin = descriptor.Origin
                };
            }

            // 3. Authorize
            var auth = _authorizer.Authorize(invocation, descriptor, new ToolBindingContext { AgentId = "unknown" });
            if (!auth.IsAuthorized)
            {
                return new ToolResult
                {
                    AgentId = invocation.AgentId,
                    Status = ToolResultStatus.Error,
                    Error = new ToolError
                    {
                        Code = ToolErrorCode.AuthError,
                        Subcode = "NotAuthorized",
                        Message = auth.Reason ?? "Authorization denied.",
                        IsRetryable = false
                    },
                    CorrelationId = correlationId,
                    ToolName = invocation.ToolName,
                    DurationMs = (int)sw.ElapsedMilliseconds,
                    Origin = descriptor.Origin
                };
            }

            // 4. Apply policy (timeout only in this slice)
            var policyResult = _policyApplier.Apply(invocation, descriptor, cancellationToken);
            var effectiveToken = policyResult.EffectiveToken;

            // 5. Execute
            var outcome = await _executor.ExecuteAsync(invocation, descriptor, effectiveToken).ConfigureAwait(false);
            ToolResult result;
            if (outcome.Result != null)
            {
                result = outcome.Result with
                {
                    AgentId = invocation.AgentId,
                    CorrelationId = correlationId,
                    ToolName = invocation.ToolName,
                    DurationMs = (int)sw.ElapsedMilliseconds,
                    ResolvedVersion = descriptor.Version,
                    PolicySnapshot = policyResult.Snapshot,
                    Origin = descriptor.Origin
                };
            }
            else
            {
                result = new ToolResult
                {
                    AgentId = invocation.AgentId,
                    Status = ToolResultStatus.Error,
                    Error = outcome.Error,
                    CorrelationId = correlationId,
                    ToolName = invocation.ToolName,
                    DurationMs = (int)sw.ElapsedMilliseconds,
                    ResolvedVersion = descriptor.Version,
                    PolicySnapshot = policyResult.Snapshot,
                    Origin = descriptor.Origin
                };
            }

            // 6. Postprocess
            var post = _postprocessor.Postprocess(result, descriptor.Contract.OutputSchema);
            return post.Result;
        }
        catch (OperationCanceledException)
        {
            return new ToolResult
            {
                AgentId = invocation.AgentId,
                Status = ToolResultStatus.Error,
                Error = new ToolError
                {
                    Code = ToolErrorCode.PolicyError,
                    Subcode = "Timeout",
                    Message = "Invocation exceeded deadline.",
                    IsRetryable = true
                },
                CorrelationId = correlationId,
                ToolName = invocation.ToolName,
                DurationMs = (int)sw.ElapsedMilliseconds,
                Origin = "unknown"
            };
        }
        catch (Exception ex)
        {
            return new ToolResult
            {
                AgentId = invocation.AgentId,
                Status = ToolResultStatus.Error,
                Error = new ToolError
                {
                    Code = ToolErrorCode.SystemError,
                    Subcode = "Unhandled",
                    Message = ex.Message,
                    Details = ex.ToString(),
                    IsRetryable = false
                },
                CorrelationId = correlationId,
                ToolName = invocation.ToolName,
                DurationMs = (int)sw.ElapsedMilliseconds,
                Origin = "unknown"
            };
        }
    }
}
