namespace RolemasterCharacterCreation.Models;

// A campaign town/city. Stores the raw Watabou/MFCG FeatureCollection JSON exactly
// as uploaded; the geometry is parsed on demand by TownMapService for rendering.
public class Town
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }

    // The uploaded city generator export (GeoJSON-style FeatureCollection).
    public required string RawJson { get; set; }

    public string? CreatedByUserId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<TownLocation> Locations { get; set; } = [];
}
