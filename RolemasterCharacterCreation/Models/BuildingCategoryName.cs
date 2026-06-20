namespace RolemasterCharacterCreation.Models;

// A preset name belonging to a building category (e.g. "Pit Trap" for Trap). Seeded from
// docs/game-data/building-category-names.json.
public class BuildingCategoryName
{
    public int Id { get; set; }

    public int BuildingCategoryId { get; set; }
    public BuildingCategory? BuildingCategory { get; set; }

    public required string Name { get; set; }
}
