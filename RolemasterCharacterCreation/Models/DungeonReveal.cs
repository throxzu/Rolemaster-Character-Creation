namespace RolemasterCharacterCreation.Models;

// Records that a single dungeon room (floor rectangle) has been revealed to players.
// Absence of a row means the room is still fogged. Mirrors WorldReveal for hex maps.
public class DungeonReveal
{
    public int Id { get; set; }

    public int DungeonMapId { get; set; }
    public DungeonMap? DungeonMap { get; set; }

    // Index of the revealed floor rectangle within the uploaded JSON's "rects" array.
    public int RectIndex { get; set; }
}
