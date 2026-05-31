namespace RolemasterCharacterCreation.Models;

public class CharacterTalent
{
    public int Id { get; set; }
    public int CharacterId { get; set; }
    public Character Character { get; set; } = null!;
    public required string TalentName { get; set; }
    public int Tier { get; set; } = 1;
    public string? Restriction { get; set; }  // e.g. "Fire" for Elemental Resistance
}
