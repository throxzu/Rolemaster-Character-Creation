namespace RolemasterCharacterCreation.Models;

/// <summary>
/// Human-readable creature description, curated from the Creature Law book and
/// seeded from <c>docs/game-data/creature-descriptions.json</c>. Stat blocks are
/// still parsed live from the markdown; only the prose lives here so it can be
/// reviewed and hand-corrected. Keyed by creature <see cref="Name"/>.
/// </summary>
public class CreatureDescription
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string Description { get; set; } = "";
}
