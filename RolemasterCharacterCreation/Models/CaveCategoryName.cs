namespace RolemasterCharacterCreation.Models;

// A preset name belonging to a cave category (e.g. "Pit Trap" for Trap). Seeded from
// docs/game-data/cave-category-names.json.
public class CaveCategoryName
{
    public int Id { get; set; }

    public int CaveCategoryId { get; set; }
    public CaveCategory? CaveCategory { get; set; }

    public required string Name { get; set; }
}
