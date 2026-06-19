namespace RolemasterCharacterCreation.Models;

/// <summary>
/// A Rolemaster Spell Law spell failure table (Tables 4-4a/b/c), one per realm —
/// Channeling, Essence or Mentalism. Each has four spell-list-type columns (names
/// held here) and roll-keyed rows of descriptive results. Seeded from
/// <c>docs/game-data/spell-failure-tables.json</c>, read-only at runtime.
/// </summary>
public class SpellFailureTable
{
    public int Id { get; set; }
    public required string Name { get; set; }

    /// <summary>Hash of the source JSON entry; lets seeding rebuild only when changed.</summary>
    public string? Signature { get; set; }

    public string Col1Name { get; set; } = "";
    public string Col2Name { get; set; } = "";
    public string Col3Name { get; set; } = "";
    public string Col4Name { get; set; } = "";

    public string[] ColumnNames => [Col1Name, Col2Name, Col3Name, Col4Name];

    public List<SpellFailureTableRow> Rows { get; set; } = [];
}
