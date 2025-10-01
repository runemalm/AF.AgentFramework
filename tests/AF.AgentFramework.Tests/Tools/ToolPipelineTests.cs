using AgentFramework.Tools.Contracts;
using AgentFramework.Tools.Pipeline;
using AgentFramework.Tools.Registry;
using Xunit;

namespace AgentFramework.Tests.Tools;

public class ToolPipelineTests
{
    private static ToolDescriptor Make(string name = "demo", string version = "1.0.0")
        => new()
        {
            Name = name,
            Version = version,
            Origin = "unknown",
            Contract = new ToolContract
            {
                Name = name,
                Version = version,
                Description = "test",
                Effect = EffectLevel.Pure,
                DefaultPolicies = new PolicyDefaults { TimeoutMs = 100 } // small default timeout for tests
            },
            Tags = new List<string>()
        };

    private sealed class StubRegistry : IToolRegistry
    {
        private readonly ToolDescriptor? _descriptor;
        public StubRegistry(ToolDescriptor? d) => _descriptor = d;
        public void Publish(ToolDescriptor descriptor) => throw new NotImplementedException();
        public bool Unpublish(string name, string version) => throw new NotImplementedException();
        public ToolDescriptor? Resolve(string name, string? versionRange, ToolBindingContext bindingContext) => _descriptor;
        public IReadOnlyList<ToolDescriptor> List(ToolBindingContext bindingContext) => _descriptor is null ? Array.Empty<ToolDescriptor>() : new[] { _descriptor };
    }

    private sealed class DenyAuthorizer : IAuthorizer
    {
        public AuthorizationOutcome Authorize(ToolInvocation invocation, ToolDescriptor descriptor, ToolBindingContext bindingContext)
            => AuthorizationOutcome.Deny("nope");
    }

    private sealed class FailValidator : IInputValidator
    {
        public ValidationOutcome Validate(object? input, object? inputSchema) => ValidationOutcome.Invalid("bad field");
    }

    private sealed class SleepExecutor : IExecutor
    {
        private readonly int _sleepMs;
        public SleepExecutor(int sleepMs) => _sleepMs = sleepMs;

        public async Task<ExecutionOutcome> ExecuteAsync(ToolInvocation invocation, ToolDescriptor descriptor, CancellationToken cancellationToken)
        {
            try
            {
                await Task.Delay(_sleepMs, cancellationToken);
                return new ExecutionOutcome { Result = new ToolResult { Status = ToolResultStatus.Ok, Output = new { ok = true } } };
            }
            catch (OperationCanceledException)
            {
                return new ExecutionOutcome { Error = new ToolError { Code = ToolErrorCode.PolicyError, Subcode = "Timeout", Message = "timeout", IsRetryable = true } };
            }
        }
    }

    private sealed class OkExecutor : IExecutor
    {
        public Task<ExecutionOutcome> ExecuteAsync(ToolInvocation invocation, ToolDescriptor descriptor, CancellationToken cancellationToken)
        {
            var res = new ToolResult
            {
                Status = ToolResultStatus.Ok,
                Output = new { ok = true }
            };
            return Task.FromResult(new ExecutionOutcome { Result = res });
        }
    }

    private sealed class AllowAll : IAuthorizer
    {
        public AuthorizationOutcome Authorize(ToolInvocation invocation, ToolDescriptor descriptor, ToolBindingContext bindingContext)
            => AuthorizationOutcome.Allow();
    }

    private sealed class PassValidator : IInputValidator
    {
        public ValidationOutcome Validate(object? input, object? inputSchema) => ValidationOutcome.Valid();
    }

    private sealed class TimeoutOnly : IPolicyApplier
    {
        public PolicyApplicationResult Apply(ToolInvocation invocation, ToolDescriptor descriptor, CancellationToken outerToken)
        {
            int? timeoutMs = descriptor.Contract.DefaultPolicies.TimeoutMs ?? 50;
            var cts = CancellationTokenSource.CreateLinkedTokenSource(outerToken);
            cts.CancelAfter(timeoutMs ?? 50);
            return new PolicyApplicationResult
            {
                Snapshot = new PolicySnapshot { TimeoutMs = timeoutMs, RetryEnabled = false },
                EffectiveToken = cts.Token
            };
        }
    }

    private sealed class NoopPost : IPostprocessor
    {
        public PostprocessOutcome Postprocess(ToolResult result, object? outputSchema) => new() { Result = result };
    }

    [Fact]
    public async Task NotFound_ReturnsContractError()
    {
        var pipeline = new ToolPipeline(
            new StubRegistry(null),
            new PassValidator(),
            new AllowAll(),
            new TimeoutOnly(),
            new OkExecutor(),
            new NoopPost());

        var result = await pipeline.InvokeAsync(new ToolInvocation { ToolName = "missing" });
        Assert.Equal(ToolResultStatus.Error, result.Status);
        Assert.NotNull(result.Error);
        Assert.Equal(ToolErrorCode.ContractError, result.Error!.Code);
        Assert.Equal("ToolNotFound", result.Error!.Subcode);
    }

    [Fact]
    public async Task ValidationFailure_ReturnsContractError()
    {
        var pipeline = new ToolPipeline(
            new StubRegistry(Make()),
            new FailValidator(),
            new AllowAll(),
            new TimeoutOnly(),
            new OkExecutor(),
            new NoopPost());

        var result = await pipeline.InvokeAsync(new ToolInvocation { ToolName = "demo" });
        Assert.Equal(ToolResultStatus.Error, result.Status);
        Assert.Equal(ToolErrorCode.ContractError, result.Error!.Code);
        Assert.Equal("ValidationFailed", result.Error!.Subcode);
        Assert.NotNull(result.Error!.Details);
    }

    [Fact]
    public async Task AuthorizationDenied_ReturnsAuthError()
    {
        var pipeline = new ToolPipeline(
            new StubRegistry(Make()),
            new PassValidator(),
            new DenyAuthorizer(),
            new TimeoutOnly(),
            new OkExecutor(),
            new NoopPost());

        var result = await pipeline.InvokeAsync(new ToolInvocation { ToolName = "demo" });
        Assert.Equal(ToolResultStatus.Error, result.Status);
        Assert.Equal(ToolErrorCode.AuthError, result.Error!.Code);
        Assert.Equal("NotAuthorized", result.Error!.Subcode);
    }

    [Fact]
    public async Task TimeoutPolicy_Enforced()
    {
        var pipeline = new ToolPipeline(
            new StubRegistry(Make(version: "1.0.0")),
            new PassValidator(),
            new AllowAll(),
            new TimeoutOnly(),
            new SleepExecutor(1000), // sleeps long; policy timeout is ~100ms
            new NoopPost());

        var result = await pipeline.InvokeAsync(new ToolInvocation { ToolName = "demo" });
        Assert.Equal(ToolResultStatus.Error, result.Status);
        Assert.Equal(ToolErrorCode.PolicyError, result.Error!.Code);
        Assert.Equal("Timeout", result.Error!.Subcode);
    }

    [Fact]
    public async Task Success_FillsMetadata()
    {
        var desc = Make(version: "1.2.3");
        var pipeline = new ToolPipeline(
            new StubRegistry(desc),
            new PassValidator(),
            new AllowAll(),
            new TimeoutOnly(),
            new OkExecutor(),
            new NoopPost());

        var result = await pipeline.InvokeAsync(new ToolInvocation { ToolName = "demo" });
        Assert.Equal(ToolResultStatus.Ok, result.Status);
        Assert.Equal("1.2.3", result.ResolvedVersion);
        Assert.Equal(desc.Origin, result.Origin);
        Assert.False(string.IsNullOrWhiteSpace(result.CorrelationId));
        Assert.NotNull(result.PolicySnapshot);
    }
}
