namespace RolemasterCharacterCreation.Services;

// Textbelt settings ("Textbelt" config section). The API key can be supplied either as the
// top-level TEXTBELT_API_KEY value (env var / user-secret) or here as "Textbelt:ApiKey" in an
// appsettings file (e.g. appsettings.Production.json, alongside the connection string). The
// sender prefers TEXTBELT_API_KEY when both are set.
public sealed class TextbeltOptions
{
    public const string SectionName = "Textbelt";

    public string BaseUrl { get; set; } = "https://textbelt.com";

    public string? ApiKey { get; set; }
}
