namespace RolemasterCharacterCreation.Models;

/// <summary>
/// A generic, read-only rulebook lookup table loaded from
/// docs/game-data/reference-tables.json. Each table carries its own
/// column headers and rows, so tables of any shape share one model and one page.
/// </summary>
public sealed class ReferenceTable
{
    /// <summary>Book table number, e.g. "5-6" or "2-5a".</summary>
    public string Number { get; set; } = "";
    public string Name { get; set; } = "";
    /// <summary>Source rulebook, e.g. "Core Law" / "Spell Law".</summary>
    public string Book { get; set; } = "";
    /// <summary>Grouping for the table list, e.g. "Stats", "Combat & Actions".</summary>
    public string Category { get; set; } = "";
    /// <summary>Optional intro / how-to-read text (plain text, newlines preserved).</summary>
    public string? Notes { get; set; }

    /// <summary>When true, the table is only shown to Gamemasters (hidden from players).</summary>
    public bool GmOnly { get; set; }
    public List<string> Columns { get; set; } = [];
    public List<List<string>> Rows { get; set; } = [];

    /// <summary>Optional secondary grids (e.g. a Modifiers table) rendered below the main table.</summary>
    public List<ReferenceSubTable> Subtables { get; set; } = [];

    /// <summary>Character-sheet skill names this table applies to (used to deep-link from the sheet).</summary>
    public List<string> Skills { get; set; } = [];

    public string DisplayTitle => string.IsNullOrEmpty(Number) ? Name : $"{Number} {Name}";
}

/// <summary>A secondary grid attached to a <see cref="ReferenceTable"/>, shown below the main one.</summary>
public sealed class ReferenceSubTable
{
    public string? Title { get; set; }
    public List<string> Columns { get; set; } = [];
    public List<List<string>> Rows { get; set; } = [];
}
