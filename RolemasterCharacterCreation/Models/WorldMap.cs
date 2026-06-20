namespace RolemasterCharacterCreation.Models;

// A campaign world/region map. Stores the raw Watabou "Perilous Shores" hex export
// (hex grid + terrain + roads/rivers/sea-routes + generated settlements). Terrain and
// routes are parsed on demand by WorldMapService; points of interest live in Locations.
public class WorldMap
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }

    public required string RawJson { get; set; }

    public string? CreatedByUserId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<WorldLocation> Locations { get; set; } = [];
}
