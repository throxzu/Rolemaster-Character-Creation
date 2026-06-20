namespace RolemasterCharacterCreation.Services;

// User-Agent heuristics for telling a phone apart from a tablet/desktop. Phones are the
// only devices the chat-only lockdown applies to, so tablets must be excluded.
public static class MobileDetection
{
    public static bool IsPhone(string? userAgent)
    {
        if (string.IsNullOrEmpty(userAgent)) return false;

        // iPad is a tablet. (Modern iPadOS reports as "Macintosh" and is treated as desktop.)
        if (Has(userAgent, "iPad")) return false;
        // Explicit "Tablet" token (many Android tablets) → not a phone.
        if (Has(userAgent, "Tablet")) return false;

        if (Has(userAgent, "iPhone") || Has(userAgent, "iPod")) return true;
        if (Has(userAgent, "Windows Phone")) return true;
        // Android phones carry "Mobile"; Android tablets omit it.
        if (Has(userAgent, "Android") && Has(userAgent, "Mobile")) return true;
        // Generic mobile token used by other small-screen browsers.
        if (Has(userAgent, "Mobi")) return true;

        return false;
    }

    private static bool Has(string ua, string token) =>
        ua.Contains(token, StringComparison.OrdinalIgnoreCase);
}
