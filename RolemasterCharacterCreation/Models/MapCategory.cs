namespace RolemasterCharacterCreation.Models;

// A GM-editable, campaign-wide palette entry used to color-code town locations
// (e.g. Tavern/Inn, Shop, Temple). Shared across all towns for consistency.
public class MapCategory
{
    public int Id { get; set; }
    public required string Name { get; set; }

    // Hex color string including the leading '#', e.g. "#E8B923".
    public required string ColorHex { get; set; }

    public int SortOrder { get; set; }

    // True for the defaults seeded at startup; false for GM-added categories.
    public bool IsBuiltIn { get; set; }

    public List<TownLocation> Locations { get; set; } = [];

    // Preset fantasy place-names offered in the building popup submenu for this type.
    public List<MapCategoryName> Names { get; set; } = [];
}
