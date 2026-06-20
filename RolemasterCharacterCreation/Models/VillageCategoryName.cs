namespace RolemasterCharacterCreation.Models;

// A preset fantasy place-name belonging to a village category (e.g. "The Plough &
// Sickle" for Tavern / Inn). Seeded from docs/game-data/village-category-names.json.
public class VillageCategoryName
{
    public int Id { get; set; }

    public int VillageCategoryId { get; set; }
    public VillageCategory? VillageCategory { get; set; }

    public required string Name { get; set; }
}
