namespace RolemasterCharacterCreation.Models;

// A GM-editable, campaign-wide legend used to mark cave features (e.g. Underground
// Lake, Crystal Vein, Beast Den). Shared across all caves for consistency.
public class CaveCategory
{
    public int Id { get; set; }
    public required string Name { get; set; }

    // Hex color string including the leading '#', e.g. "#2980B9".
    public required string ColorHex { get; set; }

    public int SortOrder { get; set; }

    // True for the defaults seeded at startup; false for GM-added categories.
    public bool IsBuiltIn { get; set; }

    // Hidden legends (traps, secret passages, ambushes…) are visible only to the GM,
    // even after the surrounding area is revealed — so players never learn the secret.
    public bool IsHidden { get; set; }

    public List<CaveLocation> Locations { get; set; } = [];

    // Preset names offered in the cell popup submenu for this type.
    public List<CaveCategoryName> Names { get; set; } = [];
}
