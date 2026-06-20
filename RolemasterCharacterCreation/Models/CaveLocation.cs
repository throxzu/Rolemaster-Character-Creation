namespace RolemasterCharacterCreation.Models;

// A color-coded marker placed on one grid square of a cave map. A region can hold
// several markers (different legends on different squares), so marks are keyed by the
// cell they sit on.
public class CaveLocation
{
    public int Id { get; set; }

    public int CaveMapId { get; set; }
    public CaveMap? CaveMap { get; set; }

    // Grid coordinates of the square this marker sits on.
    public int CellX { get; set; }
    public int CellY { get; set; }

    public int CaveCategoryId { get; set; }
    public CaveCategory? CaveCategory { get; set; }

    // Optional place name (e.g. "The Weeping Pool") shown on hover.
    public string? Label { get; set; }

    // Optional public description shown to everyone (players included) on hover.
    public string? Notes { get; set; }

    // Private GM-only notes; never shown to players or sent to their browser.
    public string? GmNotes { get; set; }

    // Markers are hidden from players by default; the GM flips this on to reveal a
    // specific legend to players (still gated by fog and never overriding a hidden type).
    public bool VisibleToPlayers { get; set; }
}
