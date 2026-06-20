namespace RolemasterCharacterCreation.Models;

// A GM-editable, campaign-wide palette entry used to color-code world-map points of
// interest (e.g. City, Town, Dungeon). Separate from the town-building MapCategory set.
public class WorldCategory
{
    public int Id { get; set; }
    public required string Name { get; set; }

    // Hex color string including the leading '#', e.g. "#C0392B".
    public required string ColorHex { get; set; }

    public int SortOrder { get; set; }
    public bool IsBuiltIn { get; set; }

    public List<WorldLocation> Locations { get; set; } = [];
}
