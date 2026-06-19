namespace RolemasterCharacterCreation.Models;

/// <summary>
/// One row of a spell failure table, keyed by roll range (<see cref="RollLow"/>–
/// <see cref="RollHigh"/>). <see cref="Col1"/>–<see cref="Col4"/> are the result
/// descriptions for the parent table's four spell-list-type columns.
/// </summary>
public class SpellFailureTableRow
{
    public int Id { get; set; }
    public int SpellFailureTableId { get; set; }
    public SpellFailureTable SpellFailureTable { get; set; } = null!;

    public int RollLow { get; set; }
    public int RollHigh { get; set; }

    public string Col1 { get; set; } = "";
    public string Col2 { get; set; } = "";
    public string Col3 { get; set; } = "";
    public string Col4 { get; set; } = "";

    public string[] Cells => [Col1, Col2, Col3, Col4];
}
