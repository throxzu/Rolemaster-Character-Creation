namespace RolemasterCharacterCreation.Models;

// A keyed, numbered note on a cave map, added by the GM. Notes are GM-only by default;
// the GM can flip one visible so players see a point-of-interest star there (number and
// text stay GM-side). Position is stored in grid units.
public class CaveNote
{
    public int Id { get; set; }

    public int CaveMapId { get; set; }
    public CaveMap? CaveMap { get; set; }

    // The number/label shown in the side panel and (for the GM) on the map.
    public string? Ref { get; set; }

    public string? Text { get; set; }

    public bool VisibleToPlayers { get; set; }

    // Grid-unit position of the keyed spot.
    public double X { get; set; }
    public double Y { get; set; }
}
