namespace RolemasterCharacterCreation.Models;

// Records that a single grid square of a cave map has been revealed to players.
// Absence of a row means the square is still fogged.
public class CaveReveal
{
    public int Id { get; set; }

    public int CaveMapId { get; set; }
    public CaveMap? CaveMap { get; set; }

    // Grid coordinates of the revealed square.
    public int CellX { get; set; }
    public int CellY { get; set; }
}
