namespace RolemasterCharacterCreation.Models;

/// <summary>
/// One row of a critical strike table, keyed by attack roll range
/// (<see cref="RollLow"/>–<see cref="RollHigh"/>). <see cref="Location"/> is the
/// body area struck (Head/Chest/Abdomen/Leg/Arm); <see cref="A"/>–<see cref="E"/>
/// are the result descriptions for each critical severity.
/// </summary>
public class CriticalTableRow
{
    public int Id { get; set; }
    public int CriticalTableId { get; set; }
    public CriticalTable CriticalTable { get; set; } = null!;

    public int RollLow { get; set; }
    public int RollHigh { get; set; }
    public string? Location { get; set; }

    public string A { get; set; } = "";
    public string B { get; set; } = "";
    public string C { get; set; } = "";
    public string D { get; set; } = "";
    public string E { get; set; } = "";
}
