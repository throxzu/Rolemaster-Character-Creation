using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;

namespace RolemasterCharacterCreation.Services;

/// <summary>
/// Sends SMS via the Textbelt HTTP API (https://docs.textbelt.com). Called over a typed
/// HttpClient — there is no official C# SDK. The API key is read from the TEXTBELT_API_KEY
/// configuration value (environment variable or user-secret) and is never stored in appsettings.
/// Use the special key "textbelt_test" to exercise the flow without sending a real text.
/// </summary>
public sealed class TextbeltSmsSender : ISmsSender
{
    private readonly HttpClient _http;
    private readonly TextbeltOptions _options;
    private readonly string? _apiKey;
    private readonly ILogger<TextbeltSmsSender> _logger;

    public TextbeltSmsSender(
        HttpClient http,
        IOptions<TextbeltOptions> options,
        IConfiguration config,
        ILogger<TextbeltSmsSender> logger)
    {
        _http = http;
        _options = options.Value;
        // Prefer the env-style TEXTBELT_API_KEY; fall back to "Textbelt:ApiKey" from appsettings.
        var envKey = config["TEXTBELT_API_KEY"];
        _apiKey = string.IsNullOrWhiteSpace(envKey) ? _options.ApiKey : envKey;
        _logger = logger;
    }

    public async Task<SmsResult> SendAsync(string phoneE164, string message, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(_apiKey))
            return new SmsResult(false, "SMS is not configured (TEXTBELT_API_KEY is missing).");

        // Strip URLs and fancy formatting so the provider doesn't reject the message.
        // Applies to every SMS sent from the solution.
        message = SmsText.Sanitize(message);

        try
        {
            var form = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["phone"] = phoneE164,
                ["message"] = message,
                ["key"] = _apiKey,
            });

            using var resp = await _http.PostAsync($"{_options.BaseUrl}/text", form, ct);
            var body = await resp.Content.ReadFromJsonAsync<TextbeltResponse>(cancellationToken: ct);

            if (body is null)
                return new SmsResult(false, $"Empty response from Textbelt (HTTP {(int)resp.StatusCode}).");

            return body.Success
                ? new SmsResult(true, QuotaRemaining: body.QuotaRemaining)
                : new SmsResult(false, body.Error ?? "Textbelt rejected the message.", body.QuotaRemaining);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Failed to send SMS to {Phone} via Textbelt", phoneE164);
            return new SmsResult(false, "Could not reach the SMS provider.");
        }
    }

    private sealed record TextbeltResponse(
        [property: JsonPropertyName("success")] bool Success,
        [property: JsonPropertyName("quotaRemaining")] int? QuotaRemaining,
        [property: JsonPropertyName("error")] string? Error);
}
