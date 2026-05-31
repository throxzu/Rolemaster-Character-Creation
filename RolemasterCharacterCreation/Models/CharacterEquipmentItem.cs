namespace RolemasterCharacterCreation.Models;

public class CharacterEquipmentItem
{
    public int Id { get; set; }
    public int CharacterId { get; set; }
    public Character? Character { get; set; }
    public required string Name { get; set; }
    public int Qty { get; set; } = 1;
}
