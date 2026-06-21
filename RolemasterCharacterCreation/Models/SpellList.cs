namespace RolemasterCharacterCreation.Models;

/// <summary>
/// A Rolemaster Spell Law spell list (chapters 6-9), e.g. "Barrier Law".
/// Grouped by <see cref="Realm"/> (Channeling/Essence/Mentalism/Hybrid) and
/// <see cref="Category"/> (Open Channeling, Cleric Base, …). Seeded from
/// <c>docs/game-data/spell-lists.json</c>, read-only at runtime.
/// </summary>
public class SpellList
{
    public int Id { get; set; }
    public required string Name { get; set; }

    public string Realm { get; set; } = "";
    public string Category { get; set; } = "";
    public string? Profession { get; set; }
    public string? Code { get; set; }

    /// <summary>When true, the list is only shown to Gamemasters (hidden from players).</summary>
    public bool GmOnly { get; set; }

    /// <summary>Hash of the source JSON entry; lets seeding rebuild only when changed.</summary>
    public string? Signature { get; set; }

    public List<Spell> Spells { get; set; } = [];
}
