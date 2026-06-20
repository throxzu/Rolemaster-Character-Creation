using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Anthropic;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace RolemasterCharacterCreation.Services;

public sealed class RulesIndexService : IHostedService
{
    private readonly RulesAssistantOptions _options;
    private readonly IEmbeddingClient _embeddings;
    private readonly IChatClient _chat;
    private readonly IConfiguration _config;
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<RulesIndexService> _logger;
    private List<RulesChunk> _chunks = [];

    public bool IsReady { get; private set; }
    public string? StartupError { get; private set; }

    public RulesIndexService(
        IEmbeddingClient embeddings,
        AnthropicClient anthropic,
        IOptions<RulesAssistantOptions> options,
        IConfiguration config,
        IWebHostEnvironment env,
        ILogger<RulesIndexService> logger)
    {
        _options = options.Value;
        _embeddings = embeddings;
        _config = config;
        _env = env;
        _logger = logger;
        _chat = anthropic.AsIChatClient(_options.ChatModel);
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _ = Task.Run(() => IndexAsync(cancellationToken), cancellationToken);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private async Task IndexAsync(CancellationToken ct)
    {
        try
        {
            // API keys must be present (env var or user-secret). Fail soft — disable the assistant.
            if (string.IsNullOrWhiteSpace(_config["ANTHROPIC_API_KEY"]) ||
                string.IsNullOrWhiteSpace(_config["VOYAGE_API_KEY"]))
            {
                StartupError = "Rules assistant disabled — set ANTHROPIC_API_KEY and VOYAGE_API_KEY";
                _logger.LogWarning("Rules assistant disabled: {Error}", StartupError);
                return;
            }

            // Resolve all source file paths.
            var root = _env.ContentRootPath;
            var primary = Path.GetFullPath(Path.Combine(root, _options.RulesFilePath));
            if (!File.Exists(primary))
            {
                StartupError = $"Rules file not found: {primary}";
                _logger.LogWarning("Rules assistant disabled: {Error}", StartupError);
                return;
            }

            var additionalPaths = (_options.AdditionalKnowledgePaths ?? [])
                .Select(p => Path.GetFullPath(Path.Combine(root, p)))
                .Where(File.Exists)
                .ToList();

            var gmOnlyPaths = (_options.GmOnlyKnowledgePaths ?? [])
                .Select(p => Path.GetFullPath(Path.Combine(root, p)))
                .Where(File.Exists)
                .ToList();

            // (path, gmOnly) for every source — player-facing material first, GM-only after.
            var sources = new[] { primary }.Concat(additionalPaths)
                .Select(p => (Path: p, GmOnly: false))
                .Concat(gmOnlyPaths.Select(p => (Path: p, GmOnly: true)))
                .ToList();

            var sw = System.Diagnostics.Stopwatch.StartNew();

            // Try loading from cache. The hash includes the embedding model so a provider/model
            // switch (different vector space) forces a re-embed rather than reusing stale vectors.
            var cachePath = _options.CachePath is { } cp
                ? Path.GetFullPath(Path.Combine(root, cp))
                : null;

            var sourceHash = ComputeSourceHash(sources.Select(s => s.Path), _options.EmbeddingModel);

            if (cachePath is not null && TryLoadCache(cachePath, sourceHash, out var cached))
            {
                _chunks = cached!;
                IsReady = true;
                _logger.LogInformation("Rules loaded from cache: {Count} chunks in {Elapsed}ms", _chunks.Count, sw.ElapsedMilliseconds);
                return;
            }

            // Parse all source files into chunks, carrying each source's GM-only flag.
            var raw = new List<(string Heading, string Text, bool GmOnly)>();
            foreach (var (path, gmOnly) in sources)
            {
                if (ct.IsCancellationRequested) return;

                var ext = Path.GetExtension(path).ToLowerInvariant();
                IReadOnlyList<(string Heading, string Text)> rawChunks = ext switch
                {
                    ".md" => MarkdownParser.ParseChunks(path, _options.MaxChunkWords),
                    _     => RulesParser.ParseChunks(path, _options.MaxChunkWords),
                };

                _logger.LogInformation("Parsed {Count} chunks from {File}{Gm}",
                    rawChunks.Count, Path.GetFileName(path), gmOnly ? " (GM-only)" : "");
                raw.AddRange(rawChunks.Select(rc => (rc.Heading, rc.Text, gmOnly)));
            }

            // Embed every chunk in one batched pass via the cloud embedding provider.
            _logger.LogInformation("Embedding {Count} chunks via {Model}…", raw.Count, _options.EmbeddingModel);
            var vectors = await _embeddings.EmbedDocumentsAsync(raw.Select(r => r.Text).ToList(), ct);

            _chunks = raw.Zip(vectors, (r, v) => new RulesChunk
            {
                Heading = r.Heading,
                Text = r.Text,
                Embedding = v,
                GmOnly = r.GmOnly,
            }).ToList();

            IsReady = true;
            _logger.LogInformation("Rules indexed: {Count} chunks in {Elapsed}ms", _chunks.Count, sw.ElapsedMilliseconds);

            if (cachePath is not null)
                SaveCache(cachePath, sourceHash, _chunks);
        }
        catch (Exception ex)
        {
            StartupError = "Rules assistant failed to initialise — see logs for details";
            _logger.LogError(ex, "Error indexing rules");
        }
    }

    public async IAsyncEnumerable<string> AskAsync(
        string question,
        IReadOnlyList<RulesChatMessage> history,
        bool includeGmContent,
        [EnumeratorCancellation] CancellationToken ct)
    {
        var qVec = await _embeddings.EmbedQueryAsync(question, ct);

        // Non-GM users never see GM-only chunks — filter before scoring so they can't surface.
        var candidates = includeGmContent ? _chunks : _chunks.Where(c => !c.GmOnly);

        var topChunks = candidates
            .Select(c => (Chunk: c, Score: CosineSimilarity(c.Embedding, qVec)))
            .Where(x => x.Score > 0.2f)
            .OrderByDescending(x => x.Score)
            .Take(_options.TopK)
            .Select(x => x.Chunk)
            .ToList();

        var excerpts = string.Join("\n\n", topChunks.Select(c =>
            $"[Section: {c.Heading}]\n{c.Text}"));

        var roleNote = includeGmContent
            ? "The current user is the Gamemaster; GM-only material (treasure, bestiary) may be included.\n\n"
            : "The current user is a player; do not reveal GM-only material such as monster stats or treasure contents.\n\n";

        var systemPrompt =
            LoadPersona() + "\n\n" + roleNote +
            "--- RULEBOOK EXCERPTS (use these for rules/mechanics questions; cite the section) ---\n" +
            excerpts;

        // History already ends with the current question (added by the UI before calling).
        var messages = new List<ChatMessage> { new(ChatRole.System, systemPrompt) };
        foreach (var msg in history.TakeLast(8))
            messages.Add(new ChatMessage(msg.Role == "user" ? ChatRole.User : ChatRole.Assistant, msg.Content));

        // Haiku 4.5 does not accept thinking/effort — plain streaming completion only.
        var chatOptions = new ChatOptions { MaxOutputTokens = _options.MaxTokens };

        await foreach (var update in _chat.GetStreamingResponseAsync(messages, chatOptions, ct))
        {
            if (!string.IsNullOrEmpty(update.Text))
                yield return update.Text;
        }
    }

    // ── Persona ───────────────────────────────────────────────────────────

    // Behaviour governed by an editable .md file, read fresh on each question so edits take
    // effect on the next message with no restart. Falls back to a built-in default if the file
    // is missing or unreadable, so the assistant never breaks on a bad/absent persona file.
    private string LoadPersona()
    {
        try
        {
            var path = Path.GetFullPath(Path.Combine(_env.ContentRootPath, _options.SystemPromptPath));
            if (File.Exists(path))
            {
                var text = File.ReadAllText(path);
                if (!string.IsNullOrWhiteSpace(text))
                    return text.Trim();
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not read persona file; using default persona");
        }
        return DefaultPersona;
    }

    private const string DefaultPersona =
        "You are an assisting gamemaster for a Rolemaster (RMU) campaign. Answer rules and " +
        "mechanics questions ONLY from the rulebook excerpts below and cite the section name; if " +
        "the excerpts don't cover it, say so and do not invent rules or numbers. For creative " +
        "requests (NPC backstories, descriptions, names, plot hooks), improvise freely and " +
        "in-character, keeping it short and usable at the table. Be concise and practical.";

    // ── Cache ─────────────────────────────────────────────────────────────

    private sealed class CacheFile
    {
        public string Hash { get; set; } = "";
        public List<CachedChunk> Chunks { get; set; } = [];
    }

    private sealed class CachedChunk
    {
        public string Heading { get; set; } = "";
        public string Text { get; set; } = "";
        public float[] Embedding { get; set; } = [];
        public bool GmOnly { get; set; }
    }

    private static bool TryLoadCache(string path, string hash, out List<RulesChunk>? chunks)
    {
        chunks = null;
        try
        {
            if (!File.Exists(path)) return false;
            var json = File.ReadAllText(path);
            var cache = JsonSerializer.Deserialize<CacheFile>(json);
            if (cache is null || cache.Hash != hash) return false;
            chunks = cache.Chunks
                .Select(c => new RulesChunk { Heading = c.Heading, Text = c.Text, Embedding = c.Embedding, GmOnly = c.GmOnly })
                .ToList();
            return true;
        }
        catch { return false; }
    }

    private void SaveCache(string path, string hash, List<RulesChunk> chunks)
    {
        try
        {
            var cache = new CacheFile
            {
                Hash = hash,
                Chunks = chunks.Select(c => new CachedChunk
                {
                    Heading   = c.Heading,
                    Text      = c.Text,
                    Embedding = c.Embedding,
                    GmOnly    = c.GmOnly
                }).ToList()
            };
            var json = JsonSerializer.Serialize(cache);
            File.WriteAllText(path, json);
        }
        catch (Exception ex)
        {
            // Non-fatal, but in production an unwritable cache means a re-embed on every start —
            // surface it so the app-pool identity can be granted write to the docs folder.
            _logger.LogWarning(ex, "Could not write embed cache to {Path}; will re-embed on next start", path);
        }
    }

    private static string ComputeSourceHash(IEnumerable<string> paths, string embeddingModel)
    {
        var sb = new StringBuilder();
        // Vector space depends on the embedding model, so it's part of the cache identity.
        sb.Append("model=").Append(embeddingModel).Append(';');
        foreach (var p in paths)
        {
            if (!File.Exists(p)) continue;
            // Hash file content (by name, not full path or timestamp) so the cache stays valid
            // when the docs are copied to a new location/host during a deploy.
            var contentHash = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(p)));
            sb.Append(Path.GetFileName(p)).Append('|').Append(contentHash).Append(';');
        }
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(sb.ToString())));
    }

    // ── Similarity ────────────────────────────────────────────────────────

    private static float CosineSimilarity(float[] a, float[] b)
    {
        float dot = 0f, na = 0f, nb = 0f;
        for (int i = 0; i < a.Length; i++)
        {
            dot += a[i] * b[i];
            na  += a[i] * a[i];
            nb  += b[i] * b[i];
        }
        return dot / (MathF.Sqrt(na) * MathF.Sqrt(nb) + 1e-8f);
    }
}
