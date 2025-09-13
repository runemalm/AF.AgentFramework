namespace AgentFramework.Kernel.Abstractions;

public readonly record struct LoopStepId(string Value)
{
    public override string ToString() => Value;
    public static implicit operator string(LoopStepId id) => id.Value;
    public static implicit operator LoopStepId(string value) => new(value);
}
