namespace RolemasterCharacterCreation.Models;

/// <summary>
/// A Rolemaster Core Law (Section 11) critical strike table, e.g. "Krush" or
/// "Heat". Rows are keyed by attack roll and carry results for severities A-E.
/// Seeded from <c>docs/game-data/critical-tables.json</c>, read-only at runtime.
/// Keyed by <see cref="Name"/>.
/// </summary>
public class CriticalTable
{
    public int Id { get; set; }
    public required string Name { get; set; }

    /// <summary>Hash of the source JSON entry; lets seeding rebuild only when changed.</summary>
    public string? Signature { get; set; }

    public List<CriticalTableRow> Rows { get; set; } = [];
}
