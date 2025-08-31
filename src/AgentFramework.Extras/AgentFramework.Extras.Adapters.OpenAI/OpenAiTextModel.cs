using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using AgentFramework.Extras.Adapters.Abstractions;

namespace AgentFramework.Extras.Adapters.OpenAI;

public sealed class OpenAiTextModel : ITextModel
{
    private readonly HttpClient _http;
    private readonly string _apiKey;

    /// <param name="apiKey">OPENAI_API_KEY</param>
    /// <param name="http">Optional shared HttpClient (BaseAddress set internally)</param>
    public OpenAiTextModel(string apiKey, HttpClient? http = null)
    {
        _apiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey));
        _http = http ?? new HttpClient();
        _http.BaseAddress = new Uri("https://api.openai.com/v1/");
        _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
        _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    public async Task<string> ChatAsync(string systemPrompt, string userPayloadJson, TextGenerationOptions options, CancellationToken ct = default)
    {
        var body = new
        {
            model = options.Model,
            messages = new object[]
            {
                new { role = "system", content = systemPrompt },
                new { role = "user",   content = userPayloadJson }
            },
            temperature = options.Temperature,
            max_tokens = options.MaxTokens
        };

        using var resp = await _http.PostAsync("chat/completions",
            new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json"), ct);
        resp.EnsureSuccessStatusCode();

        using var doc = JsonDocument.Parse(await resp.Content.ReadAsStringAsync(ct));
        return doc.RootElement.GetProperty("choices")[0]
            .GetProperty("message").GetProperty("content").GetString() ?? string.Empty;
    }
}