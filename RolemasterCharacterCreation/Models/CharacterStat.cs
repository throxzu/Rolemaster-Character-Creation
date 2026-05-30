namespace RolemasterCharacterCreation.Models;

public class CharacterStat
{
    public int Id { get; set; }
    public int CharacterId { get; set; }
    public Character Character { get; set; } = null!;
    public StatName Stat { get; set; }
    public int Temporary { get; set; }
    public int Potential { get; set; }
}
