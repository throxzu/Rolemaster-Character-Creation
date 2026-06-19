namespace RolemasterCharacterCreation.Models;

/// <summary>
/// A Rolemaster Core Law (Section 11) fumble table — "Melee Fumbles" or
/// "Ranged Fumbles". Each has five weapon-category columns (names held here) and
/// roll-keyed rows of descriptive results. Seeded from
/// <c>docs/game-data/fumble-tables.json</c>, read-only at runtime.
/// </summary>
public class FumbleTable
{
    public int Id { get; set; }
    public required string Name { get; set; }

    /// <summary>Hash of the source JSON entry; lets seeding rebuild only when changed.</summary>
    public string? Signature { get; set; }

    public string Col1Name { get; set; } = "";
    public string Col2Name { get; set; } = "";
    public string Col3Name { get; set; } = "";
    public string Col4Name { get; set; } = "";
    public string Col5Name { get; set; } = "";

    public string[] ColumnNames => [Col1Name, Col2Name, Col3Name, Col4Name, Col5Name];

    public List<FumbleTableRow> Rows { get; set; } = [];
}
