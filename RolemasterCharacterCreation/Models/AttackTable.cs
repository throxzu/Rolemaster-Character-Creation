namespace RolemasterCharacterCreation.Models;

/// <summary>
/// One Rolemaster Core Law (Chapter 10) attack table, e.g. "Arming Sword" or
/// "Ball, Cold". Holds the "first quadrant" metadata; the result grids live in
/// <see cref="Rows"/> (Small/Medium/Big) and the weapon-stat group in
/// <see cref="Weapons"/>. Seeded from <c>docs/game-data/attack-tables.json</c>
/// and read-only at runtime. Keyed by <see cref="Name"/>.
/// </summary>
public class AttackTable
{
    public int Id { get; set; }
    public required string Name { get; set; }

    /// <summary>Weapon, Natural, or Spell — used for page grouping.</summary>
    public string Category { get; set; } = "Weapon";

    /// <summary>Critical types and their letters, e.g. "Puncture(P), Slash(S), Krush(K)".</summary>
    public string CritTypes { get; set; } = "";

    public string? DisarmMod { get; set; }
    public string? SubdualMod { get; set; }

    /// <summary>Range / area-effect notes reconstructed from the metadata block.</summary>
    public string? Notes { get; set; }

    /// <summary>Hash of the source JSON entry; lets seeding rebuild a table only
    /// when its curated data actually changed.</summary>
    public string? Signature { get; set; }

    public List<AttackTableWeapon> Weapons { get; set; } = [];
    public List<AttackTableRow> Rows { get; set; } = [];
}
