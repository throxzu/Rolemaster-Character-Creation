namespace RolemasterCharacterCreation.Models;

// A keyed, numbered note on one floor of a building map, added by the GM. Notes are GM-only
// by default; the GM can flip one visible so players see a point-of-interest star there
// (number and text stay GM-side). Position is stored in grid units, local to the floor.
public class BuildingNote
{
    public int Id { get; set; }

    public int BuildingMapId { get; set; }
    public BuildingMap? BuildingMap { get; set; }

    // Which floor (0-based) this note belongs to.
    public int FloorIndex { get; set; }

    // The number/label shown in the side panel and (for the GM) on the map.
    public string? Ref { get; set; }

    public string? Text { get; set; }

    public bool VisibleToPlayers { get; set; }

    // Grid-unit position of the keyed spot (local to the floor).
    public double X { get; set; }
    public double Y { get; set; }
}
