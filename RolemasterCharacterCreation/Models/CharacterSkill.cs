namespace RolemasterCharacterCreation.Models;

public class CharacterSkill
{
    public int Id { get; set; }
    public int CharacterId { get; set; }
    public Character Character { get; set; } = null!;

    public required string Category { get; set; }
    public required string SkillName { get; set; }
    public string? Specialization { get; set; }

    public int CulturalRanks { get; set; }
    public int PurchasedRanks { get; set; }
    public int TotalRanks => CulturalRanks + PurchasedRanks;

    public bool IsProfessionalSkill { get; set; }
    public bool IsKnack { get; set; }
}
