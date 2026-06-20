namespace RolemasterCharacterCreation.Services;

public sealed class RulesAssistantOptions
{
    public const string SectionName = "RulesAssistant";

    // Answer generation — Claude (key from ANTHROPIC_API_KEY env/user-secret).
    public string ChatModel { get; init; } = "claude-haiku-4-5";
    public int MaxTokens { get; init; } = 2048;

    // Retrieval embeddings — Voyage AI (key from VOYAGE_API_KEY env/user-secret).
    public string EmbeddingModel { get; init; } = "voyage-3.5-lite";
    public string EmbeddingBaseUrl { get; init; } = "https://api.voyageai.com/v1/embeddings";

    // Editable persona / system prompt, re-read on each question (live editing).
    public string SystemPromptPath { get; init; } = "../docs/prompts/gm-assistant.md";

    // RAG index/build settings.
    public int TopK { get; init; } = 8;
    public string RulesFilePath { get; init; } = "../docs/rules/Rolemaster_Core_Law_(RMU).md";
    public int MaxChunkWords { get; init; } = 400;
    public string[] AdditionalKnowledgePaths { get; init; } = [];

    // Sources whose chunks are tagged GM-only — retrieved only for Gamemaster users.
    public string[] GmOnlyKnowledgePaths { get; init; } = [];

    public string? CachePath { get; init; }
}
