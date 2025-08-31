namespace AgentFramework.Extras.Adapters.Abstractions;

public interface ITextModel
{
    Task<string> ChatAsync(
        string systemPrompt,
        string userPayloadJson,
        TextGenerationOptions options,
        CancellationToken ct = default);
}

public sealed record TextGenerationOptions(
    string Model,
    double Temperature = 0.2,
    int MaxTokens = 800);
