namespace RolemasterCharacterCreation.Models;

public class CharacterAuditLog
{
    public int Id { get; set; }
    public int CharacterId { get; set; }
    public Character Character { get; set; } = null!;

    public string? ChangedByUserId { get; set; }
    public ApplicationUser? ChangedByUser { get; set; }

    public required string FieldName { get; set; }
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
}
