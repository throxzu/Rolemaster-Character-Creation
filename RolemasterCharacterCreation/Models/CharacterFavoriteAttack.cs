namespace RolemasterCharacterCreation.Models;

/// <summary>
/// A character's favorited attack table (player- or GM-managed), with the size
/// variant the player prefers to read. User data — never touched by seeding.
/// </summary>
public class CharacterFavoriteAttack
{
    public int Id { get; set; }

    public int CharacterId { get; set; }
    public Character Character { get; set; } = null!;

    public int AttackTableId { get; set; }
    public AttackTable AttackTable { get; set; } = null!;

    /// <summary>Small, Medium, or Big.</summary>
    public string PreferredSize { get; set; } = "Medium";

    public int SortOrder { get; set; }
}
