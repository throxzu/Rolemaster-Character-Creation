namespace RolemasterCharacterCreation.Models;

// A single color-coded place of interest on a town map. Identifies one feature by
// its layer kind and its index within that layer's coordinate array in the raw JSON.
public class TownLocation
{
    public int Id { get; set; }

    public int TownId { get; set; }
    public Town? Town { get; set; }

    // Which feature layer the index points into: "buildings", "squares" or "prisms".
    public required string FeatureKind { get; set; }

    // Zero-based index of the polygon within that layer's coordinate array.
    public int FeatureIndex { get; set; }

    public int MapCategoryId { get; set; }
    public MapCategory? MapCategory { get; set; }

    // Optional place name (e.g. "The Happy Goat") shown on hover.
    public string? Label { get; set; }

    // Optional public description of this place, shown to everyone (players included)
    // on hover. Editable by the GM via right-click.
    public string? Notes { get; set; }

    // Private GM-only notes (secrets, plot hooks). Never shown to players and never
    // sent to a player's browser; only the GM sees these on hover and in the editor.
    public string? GmNotes { get; set; }

    // Optional link to an uploaded building map. When set, double-clicking this building
    // opens that building's floor plan (for the GM and players alike).
    public int? LinkedBuildingId { get; set; }
    public BuildingMap? LinkedBuilding { get; set; }
}
