namespace RolemasterCharacterCreation.Models;

// A GM-editable, campaign-wide legend used to color-code dungeon rooms (e.g. Monster
// Lair, Treasure Vault, Trap). Shared across all dungeons for consistency.
public class DungeonCategory
{
    public int Id { get; set; }
    public required string Name { get; set; }

    // Hex color string including the leading '#', e.g. "#C0392B".
    public required string ColorHex { get; set; }

    public int SortOrder { get; set; }

    // True for the defaults seeded at startup; false for GM-added categories.
    public bool IsBuiltIn { get; set; }

    // Hidden legends (traps, secret doors, ambushes…) are visible only to the GM.
    // A room marked with a hidden category renders as plain floor for players, even
    // after the room is revealed — so players never learn the secret from the map.
    public bool IsHidden { get; set; }

    public List<DungeonLocation> Locations { get; set; } = [];

    // Preset names offered in the room popup submenu for this type.
    public List<DungeonCategoryName> Names { get; set; } = [];
}
