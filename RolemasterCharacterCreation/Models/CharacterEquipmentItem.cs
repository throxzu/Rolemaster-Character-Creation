namespace RolemasterCharacterCreation.Models;

public class CharacterEquipmentItem
{
    public int Id { get; set; }
    public int CharacterId { get; set; }
    public Character? Character { get; set; }
    public required string Name { get; set; }
    public int Qty { get; set; } = 1;

    // ── Combat bonuses ────────────────────────────────────────────────────────
    // Only equipped gear counts: something in the pack modifies nothing.

    /// <summary>Offensive bonus, applied to the weapon named by <see cref="AppliesTo"/>.</summary>
    public int ObBonus { get; set; }

    /// <summary>Defensive bonus. Applies whenever the item is equipped.</summary>
    public int DbBonus { get; set; }

    /// <summary>
    /// Weapon specialization this item's <see cref="ObBonus"/> applies to (e.g. "Broadsword").
    /// Null means it applies to every attack.
    /// </summary>
    public string? AppliesTo { get; set; }

    /// <summary>Worn or wielded, rather than merely carried.</summary>
    public bool IsEquipped { get; set; } = true;
}
