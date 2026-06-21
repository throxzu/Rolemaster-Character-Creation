namespace RolemasterCharacterCreation.Models;

/// <summary>
/// A named example magic item from Treasure Law chapter 6 ("Magic Item Compendium").
/// Loaded read-only from <c>docs/game-data/magic-items.json</c> and grouped by
/// <see cref="Category"/> (e.g. "Daily Items", "Melee Weapons"). Gamemaster-only data.
/// </summary>
public sealed class MagicItem
{
    public string Name { get; set; } = "";
    /// <summary>Grouping for the item list, e.g. "Armor &amp; Shields", "Jewelry".</summary>
    public string Category { get; set; } = "";
    public string Description { get; set; } = "";
    /// <summary>The enchantment recipe (Work/spell list, Days, cost). May be empty.</summary>
    public string Recipe { get; set; } = "";
    /// <summary>Item level, e.g. "10" or "5/6". May be empty.</summary>
    public string Level { get; set; } = "";
    /// <summary>Standard cost, e.g. "288 sp". May be empty.</summary>
    public string Cost { get; set; } = "";
    /// <summary>Working days to create. May be empty.</summary>
    public string Days { get; set; } = "";

    /// <summary>True for GM-generated (LLM) items rather than rulebook items; shown with a "Created" tag.</summary>
    public bool Created { get; set; }
}
