namespace RolemasterCharacterCreation.Services;

// Outcome of a send attempt. Never throws for an API-level failure (bad number, no quota,
// missing key) — callers inspect Success and can fall back (e.g. show the password to the GM).
public sealed record SmsResult(bool Success, string? Error = null, int? QuotaRemaining = null);

public interface ISmsSender
{
    Task<SmsResult> SendAsync(string phoneE164, string message, CancellationToken ct = default);
}
