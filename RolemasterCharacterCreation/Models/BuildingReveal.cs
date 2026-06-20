namespace RolemasterCharacterCreation.Models;

// Records that a single grid square of one floor of a building map has been revealed to
// players. Absence of a row means the square is still fogged.
public class BuildingReveal
{
    public int Id { get; set; }

    public int BuildingMapId { get; set; }
    public BuildingMap? BuildingMap { get; set; }

    // Which floor (0-based) the revealed square is on.
    public int FloorIndex { get; set; }

    // Grid coordinates of the revealed square (local to the floor).
    public int CellX { get; set; }
    public int CellY { get; set; }
}
