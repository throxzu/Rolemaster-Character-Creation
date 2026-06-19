namespace RolemasterCharacterCreation.Models;

/// <summary>
/// One row of a fumble table, keyed by roll range (<see cref="RollLow"/>–
/// <see cref="RollHigh"/>). <see cref="Col1"/>–<see cref="Col5"/> are the result
/// descriptions for the parent table's five weapon-category columns.
/// </summary>
public class FumbleTableRow
{
    public int Id { get; set; }
    public int FumbleTableId { get; set; }
    public FumbleTable FumbleTable { get; set; } = null!;

    public int RollLow { get; set; }
    public int RollHigh { get; set; }

    public string Col1 { get; set; } = "";
    public string Col2 { get; set; } = "";
    public string Col3 { get; set; } = "";
    public string Col4 { get; set; } = "";
    public string Col5 { get; set; } = "";

    public string[] Cells => [Col1, Col2, Col3, Col4, Col5];
}
