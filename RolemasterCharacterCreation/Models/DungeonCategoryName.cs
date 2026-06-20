namespace RolemasterCharacterCreation.Models;

// A preset name belonging to a dungeon category (e.g. "Pit Trap" for Trap). Seeded
// from docs/game-data/dungeon-category-names.json.
public class DungeonCategoryName
{
    public int Id { get; set; }

    public int DungeonCategoryId { get; set; }
    public DungeonCategory? DungeonCategory { get; set; }

    public required string Name { get; set; }
}
