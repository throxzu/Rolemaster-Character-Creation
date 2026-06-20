namespace RolemasterCharacterCreation.Services;

/// <summary>
/// Cloud embedding provider used for RAG retrieval. Kept behind an interface so the
/// provider (Voyage, OpenAI, …) is not load-bearing on the rest of the assistant.
/// </summary>
public interface IEmbeddingClient
{
    /// Embed a single user question (uses the provider's "query" input type).
    Task<float[]> EmbedQueryAsync(string text, CancellationToken ct);

    /// Embed a batch of rulebook chunks for the index build (uses "document" input type).
    Task<IReadOnlyList<float[]>> EmbedDocumentsAsync(IReadOnlyList<string> texts, CancellationToken ct);
}
