namespace RolemasterCharacterCreation.Models;

// A campaign cave. Stores the uploaded SVG map exactly as drawn (e.g. a Watabou
// "Caves" export); it is displayed unchanged with an interactive square grid overlaid
// for fog of war and legend placement (a cave is one organic shape, not rooms).
public class CaveMap
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }

    // The uploaded SVG markup, shown as-is beneath the interactive overlay.
    public required string RawSvg { get; set; }

    public string? CreatedByUserId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<CaveLocation> Locations { get; set; } = [];
}
