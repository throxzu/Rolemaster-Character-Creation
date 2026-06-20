namespace RolemasterCharacterCreation.Models;

// A keyed, numbered description on a dungeon map (e.g. "1. A rear entrance into the
// archive."). Seeded from the uploaded file's notes, then editable by the GM —
// add, edit and remove. Position is stored in grid units.
public class DungeonNote
{
    public int Id { get; set; }

    public int DungeonMapId { get; set; }
    public DungeonMap? DungeonMap { get; set; }

    // The number/label shown in the room and on the callout (e.g. "1", "2a").
    public string? Ref { get; set; }

    public string? Text { get; set; }

    // Keyed notes are GM-only by default; the GM flips this on to let players see a
    // point-of-interest star here (still gated by fog; number and text stay GM-only).
    public bool VisibleToPlayers { get; set; }

    // Grid-unit position of the keyed spot (the callout points here).
    public double X { get; set; }
    public double Y { get; set; }
}
