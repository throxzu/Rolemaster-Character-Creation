namespace RolemasterCharacterCreation.Models;

// A hex revealed to players on a world map. World maps start fully fogged (no rows);
// the GM adds a row per hex as the party explores. A hex is visible to players iff a
// matching WorldReveal exists.
public class WorldReveal
{
    public int Id { get; set; }

    public int WorldMapId { get; set; }
    public WorldMap? WorldMap { get; set; }

    public int HexQ { get; set; }
    public int HexR { get; set; }
}
