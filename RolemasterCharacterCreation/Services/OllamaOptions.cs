namespace RolemasterCharacterCreation.Services;

public sealed class OllamaOptions
{
    public const string SectionName = "Ollama";
    public string BaseUrl { get; init; } = "http://localhost:11434";
    public string ChatModel { get; init; } = "llama3.2";
    public string EmbeddingModel { get; init; } = "nomic-embed-text";
    public int TopK { get; init; } = 12;
    public string RulesFilePath { get; init; } = "../docs/rules/Rolemaster_Core_Law_(RMU).md";
    public int MaxChunkWords { get; init; } = 400;
    public string[] AdditionalKnowledgePaths { get; init; } = [];
    public string[] FullInjectPaths { get; init; } = [];
    public string? CachePath { get; init; }
}
