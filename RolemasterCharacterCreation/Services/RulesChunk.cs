namespace RolemasterCharacterCreation.Services;

public sealed class RulesChunk
{
    public required string Heading { get; init; }
    public required string Text { get; init; }
    public required float[] Embedding { get; init; }

    // True for chunks sourced from GM-only material (Treasure/Creature Law); excluded from
    // retrieval for non-GM users.
    public bool GmOnly { get; init; }
}
