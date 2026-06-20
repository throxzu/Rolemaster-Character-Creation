namespace RolemasterCharacterCreation.Models;

// A campaign dungeon. Stores the raw Watabou One-Page Dungeon JSON exactly as
// uploaded; the geometry is parsed on demand by DungeonMapService for rendering.
public class DungeonMap
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }

    // The uploaded Watabou One-Page Dungeon export (rects / doors / notes on a grid).
    public required string RawJson { get; set; }

    public string? CreatedByUserId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<DungeonLocation> Locations { get; set; } = [];
}
