namespace RolemasterCharacterCreation.Models;

// A preset fantasy place-name belonging to a map category (e.g. "The Happy Goat"
// for Tavern / Inn). Seeded from docs/game-data/map-category-names.json.
public class MapCategoryName
{
    public int Id { get; set; }

    public int MapCategoryId { get; set; }
    public MapCategory? MapCategory { get; set; }

    public required string Name { get; set; }
}
