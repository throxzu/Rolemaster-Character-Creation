namespace RolemasterCharacterCreation.Models;

// A campaign town/city. Stores the raw Watabou/MFCG FeatureCollection JSON exactly
// as uploaded; the geometry is parsed on demand by TownMapService for rendering.
public class Town
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }

    // The city generator export (GeoJSON-style FeatureCollection). The GM may reshape, add
    // and remove buildings, walls and roads, so this is the *current* geometry rather than
    // what was uploaded.
    public required string RawJson { get; set; }

    // The export exactly as it was imported, written once and never again, so an editing
    // session can always be undone wholesale. Null only for towns imported before the map
    // editor existed; those get it filled in the first time they are opened.
    public string? OriginalJson { get; set; }

    public string? CreatedByUserId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<TownLocation> Locations { get; set; } = [];
}
