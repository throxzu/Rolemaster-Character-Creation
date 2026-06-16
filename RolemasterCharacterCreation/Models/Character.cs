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
    public int CurrentXp { get; set; } = 0;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public string? UserId { get; set; }
    public ApplicationUser? User { get; set; }

    public int WizardStep { get; set; } = 0;
    public int RaceBonusDp { get; set; } = 0;
    public int? HeightCm { get; set; }
    public int? WeightKg { get; set; }
    public string? Gender { get; set; }
    public string? Build { get; set; }
    public int? Age { get; set; }
    public string? HairColor { get; set; }
    public string? EyeColor { get; set; }

    // Armor selection (Equipment step)
    public int  ArmorType     { get; set; } = 1;   // 1–10
    public bool ArmorFullSuit { get; set; }         // true = full suit covers head/arms/legs too
    public string? HelmetGrade    { get; set; }     // null / "Light" / "Medium" / "Heavy"
    public string? VambracesGrade { get; set; }
    public string? GreavesGrade   { get; set; }
    public string? ShieldType     { get; set; }     // null / "Small" / "Medium" / "Large"

    // Comma-separated chosen professional skill names (10 pro + 2 knacks = 12 entries prefixed "PRO:" or "KNA:")
    public string? ProfessionalSkillData { get; set; }

    // Frozen snapshots set when Level Up is clicked; cleared when wizard finishes.
    // Prevents gaming DP budget or stat gains by restarting the wizard mid-level-up.
    public string? LevelUpBaselineJson { get; set; }
    public string? StatBaselineJson    { get; set; }

    public List<CharacterStat> Stats { get; set; } = [];
    public List<CharacterSkill> Skills { get; set; } = [];
    public List<CharacterTalent> Talents { get; set; } = [];
    public List<CharacterEquipmentItem> EquipmentItems { get; set; } = [];
    public List<CharacterAuditLog> AuditLogs { get; set; } = [];
}
