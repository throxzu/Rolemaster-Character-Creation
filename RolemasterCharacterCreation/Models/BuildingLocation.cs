namespace RolemasterCharacterCreation.Models;

// A color-coded marker placed on one grid square of one floor of a building map. A floor
// can hold several markers (different legends on different squares), so marks are keyed by
// the floor + cell they sit on.
public class BuildingLocation
{
    public int Id { get; set; }

    public int BuildingMapId { get; set; }
    public BuildingMap? BuildingMap { get; set; }

    // Which floor (0-based, matching BuildingMapService.Floor.Index) this mark belongs to.
    public int FloorIndex { get; set; }

    // Grid coordinates of the square this marker sits on (local to the floor).
    public int CellX { get; set; }
    public int CellY { get; set; }

    public int BuildingCategoryId { get; set; }
    public BuildingCategory? BuildingCategory { get; set; }

    // Optional place name (e.g. "Master Bedroom") shown on hover.
    public string? Label { get; set; }

    // Optional public description shown to everyone (players included) on hover.
    public string? Notes { get; set; }

    // Private GM-only notes; never shown to players or sent to their browser.
    public string? GmNotes { get; set; }

    // Markers are hidden from players by default; the GM flips this on to reveal a
    // specific legend to players (still gated by fog and never overriding a hidden type).
    public bool VisibleToPlayers { get; set; }
}
