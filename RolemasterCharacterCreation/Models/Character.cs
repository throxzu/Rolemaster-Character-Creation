namespace RolemasterCharacterCreation.Models;

public class Character
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? Race { get; set; }
    public string? Culture { get; set; }
    public string? Profession { get; set; }
    public string? Realm { get; set; }
    public int Level { get; set; } = 1;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public string? UserId { get; set; }
    public ApplicationUser? User { get; set; }

    public int WizardStep { get; set; } = 0;
    public int RaceBonusDp { get; set; } = 0;

    // Comma-separated chosen professional skill names (10 pro + 2 knacks = 12 entries prefixed "PRO:" or "KNA:")
    public string? ProfessionalSkillData { get; set; }

    public List<CharacterStat> Stats { get; set; } = [];
    public List<CharacterSkill> Skills { get; set; } = [];
    public List<CharacterAuditLog> AuditLogs { get; set; } = [];
}
