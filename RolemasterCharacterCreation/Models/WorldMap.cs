namespace RolemasterCharacterCreation.Models;

// A campaign world/region map. Stores the raw Watabou "Perilous Shores" hex export
// (hex grid + terrain + roads/rivers/sea-routes + generated settlements). Terrain and
// routes are parsed on demand by WorldMapService; points of interest live in Locations.
public class WorldMap
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }

    // The hex export. The GM may repaint terrain and trace roads, rivers and sea routes, so
    // this is the *current* map rather than what was uploaded.
    public required string RawJson { get; set; }

    // The export exactly as it was imported, written once and never again, so an editing
    // session can always be undone wholesale. Null only for maps imported before the map
    // editor existed; those get it filled in the first time they are opened.
    public string? OriginalJson { get; set; }

    public string? CreatedByUserId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<WorldLocation> Locations { get; set; } = [];
}
