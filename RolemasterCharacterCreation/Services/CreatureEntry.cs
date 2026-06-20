namespace RolemasterCharacterCreation.Services;

public sealed class CreatureEntry
{
    public string Name          { get; init; } = "";
    public string CategoryGroup { get; init; } = "";
    public string Category      { get; init; } = "";
    public string Archetype     { get; init; } = "";
    public string Size          { get; init; } = "";
    public string Armor         { get; init; } = "";
    public string Heal          { get; init; } = "";
    public string Treasure      { get; init; } = "";
    public string Realm         { get; init; } = "";
    public string Misc          { get; init; } = "";
    public int[]  StatBonuses   { get; init; } = new int[10];
    public string Spells        { get; init; } = "";
    public string TalentsFlaws  { get; init; } = "";
    public string Description   { get; init; } = "";

    public static readonly string[] StatLabels = ["AG", "CO", "EM", "IN", "ME", "PR", "QU", "RE", "SD", "ST"];
}
