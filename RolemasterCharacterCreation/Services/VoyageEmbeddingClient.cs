using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;

namespace RolemasterCharacterCreation.Services;

/// <summary>
/// Voyage AI embeddings (https://docs.voyageai.com). Called over raw HTTP — Voyage has no
/// official C# SDK. The API key is read from the VOYAGE_API_KEY configuration value
/// (environment variable or user-secret); it is never stored in appsettings.
/// </summary>
public sealed class VoyageEmbeddingClient : IEmbeddingClient
{
    // Voyage caps inputs and tokens per request; keep batches modest so a single request
    // stays well under the per-request token ceiling (and the free tier's tight TPM).
    private const int BatchSize = 64;

    // 429 retry policy (honours Retry-After when present, else exponential backoff).
    private const int MaxAttempts = 6;

    private readonly HttpClient _http;
    private readonly RulesAssistantOptions _options;

    public VoyageEmbeddingClient(HttpClient http, IOptions<RulesAssistantOptions> options, IConfiguration config)
    {
        _http = http;
        _options = options.Value;

        var apiKey = config["VOYAGE_API_KEY"];
        if (!string.IsNullOrWhiteSpace(apiKey))
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
    }

    public async Task<float[]> EmbedQueryAsync(string text, CancellationToken ct)
    {
        var result = await EmbedAsync([text], "query", ct);
        return result[0];
    }

    public async Task<IReadOnlyList<float[]>> EmbedDocumentsAsync(IReadOnlyList<string> texts, CancellationToken ct)
    {
        var all = new List<float[]>(texts.Count);
        for (int i = 0; i < texts.Count; i += BatchSize)
        {
            var batch = texts.Skip(i).Take(BatchSize).ToList();
            all.AddRange(await EmbedAsync(batch, "document", ct));
        }
        return all;
    }

    private async Task<IReadOnlyList<float[]>> EmbedAsync(IReadOnlyList<string> input, string inputType, CancellationToken ct)
    {
        var request = new VoyageRequest(_options.EmbeddingModel, input, inputType);

        for (int attempt = 1; ; attempt++)
        {
            using var resp = await _http.PostAsJsonAsync(_options.EmbeddingBaseUrl, request, ct);

            // Voyage rate-limits aggressively on the free tier; back off and retry on 429.
            if (resp.StatusCode == HttpStatusCode.TooManyRequests && attempt < MaxAttempts)
            {
                var delay = resp.Headers.RetryAfter?.Delta
                            ?? TimeSpan.FromSeconds(Math.Min(60, Math.Pow(2, attempt)));
                await Task.Delay(delay, ct);
                continue;
            }

            resp.EnsureSuccessStatusCode();

            var body = await resp.Content.ReadFromJsonAsync<VoyageResponse>(ct)
                       ?? throw new InvalidOperationException("Empty embedding response from Voyage");

            // The API echoes each input's index; order defensively so vectors line up with texts.
            return body.Data
                .OrderBy(d => d.Index)
                .Select(d => d.Embedding)
                .ToList();
        }
    }

    private sealed record VoyageRequest(
        [property: JsonPropertyName("model")] string Model,
        [property: JsonPropertyName("input")] IReadOnlyList<string> Input,
        [property: JsonPropertyName("input_type")] string InputType);

    private sealed record VoyageResponse(
        [property: JsonPropertyName("data")] List<VoyageDatum> Data);

    private sealed record VoyageDatum(
        [property: JsonPropertyName("embedding")] float[] Embedding,
        [property: JsonPropertyName("index")] int Index);
}
