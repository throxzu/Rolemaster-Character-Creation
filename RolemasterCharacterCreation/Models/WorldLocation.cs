namespace RolemasterCharacterCreation.Models;

// A point of interest placed on a world map at a hex (q, r). Auto-imported from the
// file's generated settlements/sites, or added by the GM.
public class WorldLocation
{
    public int Id { get; set; }

    public int WorldMapId { get; set; }
    public WorldMap? WorldMap { get; set; }

    public int HexQ { get; set; }
    public int HexR { get; set; }

    public int WorldCategoryId { get; set; }
    public WorldCategory? WorldCategory { get; set; }

    // Place name shown on the map and on hover.
    public string? Label { get; set; }

    // Public description shown to everyone on hover.
    public string? Notes { get; set; }

    // Private GM-only notes; never shown to players or sent to their browser.
    public string? GmNotes { get; set; }

    // Optional link to an uploaded town map. When set, clicking this POI opens that town.
    public int? LinkedTownId { get; set; }
    public Town? LinkedTown { get; set; }

    // Optional link to an uploaded village map. When set, clicking this POI opens that village.
    public int? LinkedVillageId { get; set; }
    public Village? LinkedVillage { get; set; }

    // Optional link to an uploaded dungeon map. When set, clicking this POI opens that dungeon.
    public int? LinkedDungeonId { get; set; }
    public DungeonMap? LinkedDungeon { get; set; }

    // Optional link to an uploaded cave map. When set, clicking this POI opens that cave.
    public int? LinkedCaveId { get; set; }
    public CaveMap? LinkedCave { get; set; }

    // Optional link to an uploaded building map. When set, clicking this POI opens that building.
    public int? LinkedBuildingId { get; set; }
    public BuildingMap? LinkedBuilding { get; set; }
}
