namespace RolemasterCharacterCreation.Models;

/// <summary>
/// A weapon listed in an attack table's stat group (e.g. Arming Sword, Short
/// Sword and Longsword all share the Arming Sword table at different sizes).
/// Drives favorites auto-suggest by matching <see cref="Name"/> against a
/// character's weapon skills / equipment, and supplies the default size via
/// <see cref="SizeMod"/> (−1 → Small, +0 → Medium, +1 → Big).
/// </summary>
public class AttackTableWeapon
{
    public int Id { get; set; }
    public int AttackTableId { get; set; }
    public AttackTable AttackTable { get; set; } = null!;

    public required string Name { get; set; }
    public string? SizeMod { get; set; }
    public string? Length { get; set; }
    public string? Strength { get; set; }
    public string? Weight { get; set; }
    public string? Fumble { get; set; }
}
