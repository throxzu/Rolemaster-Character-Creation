namespace RolemasterCharacterCreation.Models;

/// <summary>
/// One spell on a <see cref="SpellList"/>, keyed by its <see cref="Level"/>.
/// Carries the table attributes (area of effect, duration, range, type) and the
/// prose <see cref="Description"/>.
/// </summary>
public class Spell
{
    public int Id { get; set; }
    public int SpellListId { get; set; }
    public SpellList SpellList { get; set; } = null!;

    public int Level { get; set; }
    public string Name { get; set; } = "";
    public string AreaOfEffect { get; set; } = "";
    public string Duration { get; set; } = "";
    public string Range { get; set; } = "";
    public string Type { get; set; } = "";
    public string Description { get; set; } = "";
}
