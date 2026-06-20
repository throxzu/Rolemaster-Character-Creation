namespace RolemasterCharacterCreation.Models;

// A GM-editable, campaign-wide legend used to mark building features (e.g. Bedroom,
// Kitchen, Cellar). Shared across all buildings for consistency.
public class BuildingCategory
{
    public int Id { get; set; }
    public required string Name { get; set; }

    // Hex color string including the leading '#', e.g. "#2980B9".
    public required string ColorHex { get; set; }

    public int SortOrder { get; set; }

    // True for the defaults seeded at startup; false for GM-added categories.
    public bool IsBuiltIn { get; set; }

    // Hidden legends (traps, secret doors, hidden caches…) are visible only to the GM,
    // even after the surrounding area is revealed — so players never learn the secret.
    public bool IsHidden { get; set; }

    public List<BuildingLocation> Locations { get; set; } = [];

    // Preset names offered in the cell popup submenu for this type.
    public List<BuildingCategoryName> Names { get; set; } = [];
}
