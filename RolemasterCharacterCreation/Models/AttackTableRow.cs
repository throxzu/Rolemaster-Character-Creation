namespace RolemasterCharacterCreation.Models;

/// <summary>
/// One result row of an attack table for a given size variant. Keyed by attack
/// roll range (<see cref="RollLow"/>–<see cref="RollHigh"/>); the ten cells map
/// to Armor Types 1–10. Each cell is a raw result string such as "26FP" (26
/// hits, severity F, critical type P); null = no result.
/// </summary>
public class AttackTableRow
{
    public int Id { get; set; }
    public int AttackTableId { get; set; }
    public AttackTable AttackTable { get; set; } = null!;

    /// <summary>Small, Medium, or Big.</summary>
    public required string Size { get; set; }

    public int RollLow { get; set; }
    public int RollHigh { get; set; }

    public string? At1 { get; set; }
    public string? At2 { get; set; }
    public string? At3 { get; set; }
    public string? At4 { get; set; }
    public string? At5 { get; set; }
    public string? At6 { get; set; }
    public string? At7 { get; set; }
    public string? At8 { get; set; }
    public string? At9 { get; set; }
    public string? At10 { get; set; }

    public string?[] Cells =>
        [At1, At2, At3, At4, At5, At6, At7, At8, At9, At10];
}
