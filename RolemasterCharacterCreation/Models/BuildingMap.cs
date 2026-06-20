namespace RolemasterCharacterCreation.Models;

// A campaign building. Stores the uploaded SVG map exactly as drawn (e.g. a Watabou
// "lonely house" / building export); it is a horizontal strip of square floors shown one
// at a time, each with an interactive square grid overlaid for fog of war and legends.
public class BuildingMap
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }

    // The uploaded SVG markup, shown as-is beneath the interactive overlay.
    public required string RawSvg { get; set; }

    public string? CreatedByUserId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<BuildingLocation> Locations { get; set; } = [];
}
