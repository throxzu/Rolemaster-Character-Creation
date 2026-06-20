namespace RolemasterCharacterCreation.Models;

// A GM-editable, campaign-wide palette entry used to color-code village locations
// (e.g. Tavern/Inn, General Store, Mill). Shared across all villages for consistency,
// and kept separate from the town palette so each can have its own legend.
public class VillageCategory
{
    public int Id { get; set; }
    public required string Name { get; set; }

    // Hex color string including the leading '#', e.g. "#E67E22".
    public required string ColorHex { get; set; }

    public int SortOrder { get; set; }

    // True for the defaults seeded at startup; false for GM-added categories.
    public bool IsBuiltIn { get; set; }

    public List<VillageLocation> Locations { get; set; } = [];

    // Preset fantasy place-names offered in the building popup submenu for this type.
    public List<VillageCategoryName> Names { get; set; } = [];
}
