namespace RolemasterCharacterCreation.Models;

// A color-coded marker placed on one grid square of a dungeon map. A room can hold
// several markers (different legends in different squares), so marks are keyed by the
// cell they sit on rather than by the whole room.
public class DungeonLocation
{
    public int Id { get; set; }

    public int DungeonMapId { get; set; }
    public DungeonMap? DungeonMap { get; set; }

    // Grid coordinates of the square this marker sits on.
    public int CellX { get; set; }
    public int CellY { get; set; }

    public int DungeonCategoryId { get; set; }
    public DungeonCategory? DungeonCategory { get; set; }

    // Optional place name (e.g. "Sundered Tomb") shown on hover.
    public string? Label { get; set; }

    // Optional public description shown to everyone (players included) on hover.
    public string? Notes { get; set; }

    // Markers are hidden from players by default; the GM flips this on to reveal a
    // specific legend to players (still gated by fog and never overriding a hidden type).
    public bool VisibleToPlayers { get; set; }

    // Private GM-only notes; never shown to players or sent to their browser.
    public string? GmNotes { get; set; }
}
