using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OllamaSharp;
using OllamaSharp.Models.Chat;

namespace RolemasterCharacterCreation.Services;

public sealed class RulesIndexService : IHostedService
{
    private readonly OllamaOptions _options;
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<RulesIndexService> _logger;
    private readonly OllamaApiClient _embedClient;
    private readonly OllamaApiClient _chatClient;
    private List<RulesChunk> _chunks = [];
    private List<(string FileName, string Text)> _fullInjectTexts = [];

    public bool IsReady { get; private set; }
    public string? StartupError { get; private set; }

    public RulesIndexService(IOptions<OllamaOptions> options, IWebHostEnvironment env, ILogger<RulesIndexService> logger)
    {
        _options = options.Value;
        _env = env;
        _logger = logger;
        _embedClient = new OllamaApiClient(new Uri(_options.BaseUrl), _options.EmbeddingModel);
        _chatClient  = new OllamaApiClient(new Uri(_options.BaseUrl), _options.ChatModel);
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _ = Task.Run(() => IndexAsync(cancellationToken), cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private async Task IndexAsync(CancellationToken ct)
    {
        try
        {
            // Resolve all source file paths
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

            var allPaths = new[] { primary }.Concat(additionalPaths).ToList();

            var fullInjectPaths = (_options.FullInjectPaths ?? [])
                .Select(p => Path.GetFullPath(Path.Combine(root, p)))
                .Where(File.Exists)
                .ToList();

            _fullInjectTexts = fullInjectPaths
                .Select(p => (Path.GetFileName(p), File.ReadAllText(p)))
                .ToList();

            // Ping Ollama
            using var pingCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            pingCts.CancelAfter(TimeSpan.FromSeconds(8));
            try
            {
                await _embedClient.ListLocalModelsAsync(pingCts.Token);
            }
            catch (Exception ex) when (ex is not OperationCanceledException { CancellationToken.IsCancellationRequested: true })
            {
                StartupError = $"Ollama not reachable at {_options.BaseUrl} — rules assistant disabled";
                _logger.LogWarning("Rules assistant disabled: {Error}", StartupError);
                return;
            }

            var sw = System.Diagnostics.Stopwatch.StartNew();

            // Try loading from cache
            var cachePath = _options.CachePath is { } cp
                ? Path.GetFullPath(Path.Combine(root, cp))
                : null;

            var sourceHash = ComputeSourceHash(allPaths.Concat(fullInjectPaths));

            if (cachePath is not null && TryLoadCache(cachePath, sourceHash, out var cached))
            {
                _chunks = cached!;
                IsReady = true;
                _logger.LogInformation("Rules loaded from cache: {Count} chunks in {Elapsed}ms", _chunks.Count, sw.ElapsedMilliseconds);
                _logger.LogInformation("Full-inject files: {Names}", string.Join(", ", _fullInjectTexts.Select(f => f.FileName)));
                return;
            }

            // Parse and embed all source files
            var embedded = new List<RulesChunk>();

            foreach (var path in allPaths)
            {
                if (ct.IsCancellationRequested) return;

                var ext = Path.GetExtension(path).ToLowerInvariant();
                IReadOnlyList<(string Heading, string Text)> rawChunks = ext switch
                {
                    ".md" => MarkdownParser.ParseChunks(path, _options.MaxChunkWords),
                    _     => RulesParser.ParseChunks(path, _options.MaxChunkWords),
                };

                _logger.LogInformation("Parsed {Count} chunks from {File}, embedding…",
                    rawChunks.Count, Path.GetFileName(path));

                foreach (var (heading, text) in rawChunks)
                {
                    if (ct.IsCancellationRequested) return;
                    var resp = await _embedClient.EmbedAsync(text, ct);
                    embedded.Add(new RulesChunk { Heading = heading, Text = text, Embedding = resp.Embeddings[0] });
                }
            }

            _chunks = embedded;
            IsReady = true;
            _logger.LogInformation("Rules indexed: {Count} chunks in {Elapsed}ms", _chunks.Count, sw.ElapsedMilliseconds);
            _logger.LogInformation("Full-inject files: {Names}", string.Join(", ", _fullInjectTexts.Select(f => f.FileName)));

            // Save cache
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
        [EnumeratorCancellation] CancellationToken ct)
    {
        var qResp = await _embedClient.EmbedAsync(question, ct);
        var qVec = qResp.Embeddings[0];

        var topChunks = _chunks
            .Select(c => (Chunk: c, Score: CosineSimilarity(c.Embedding, qVec)))
            .Where(x => x.Score > 0.2f)
            .OrderByDescending(x => x.Score)
            .Take(_options.TopK)
            .Select(x => x.Chunk)
            .ToList();

        var excerpts = string.Join("\n\n", topChunks.Select(c =>
            $"[Section: {c.Heading}]\n{c.Text}"));

        var fullInjectBlock = _fullInjectTexts.Count > 0
            ? "--- REFERENCE DATA (complete, authoritative) ---\n" +
              string.Join("\n\n", _fullInjectTexts.Select(f => $"[File: {f.FileName}]\n{f.Text}")) +
              "\n\n"
            : "";

        var systemPrompt =
            "You are a Rolemaster rules assistant. Answer questions based ONLY on the following rulebook excerpts. " +
            "Be specific and cite the section name. If a table is present in the excerpts, read it carefully — " +
            "the first row is column headers, subsequent rows are data. " +
            "If the answer is not in the excerpts, say so clearly.\n\n" +
            fullInjectBlock +
            "--- RULEBOOK EXCERPTS ---\n" + excerpts;

        var chat = new Chat(_chatClient, systemPrompt);
        chat.Model = _options.ChatModel;

        foreach (var msg in history.TakeLast(8))
        {
            var role = msg.Role == "user" ? ChatRole.User : ChatRole.Assistant;
            chat.Messages.Add(new Message(role, msg.Content));
        }

        await foreach (var token in chat.SendAsync(question, ct))
            yield return token;
    }

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
                .Select(c => new RulesChunk { Heading = c.Heading, Text = c.Text, Embedding = c.Embedding })
                .ToList();
            return true;
        }
        catch { return false; }
    }

    private static void SaveCache(string path, string hash, List<RulesChunk> chunks)
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
                    Embedding = c.Embedding
                }).ToList()
            };
            var json = JsonSerializer.Serialize(cache);
            File.WriteAllText(path, json);
        }
        catch { /* non-fatal — next restart re-embeds */ }
    }

    private static string ComputeSourceHash(IEnumerable<string> paths)
    {
        using var sha = SHA256.Create();
        var sb = new StringBuilder();
        foreach (var p in paths)
        {
            if (!File.Exists(p)) continue;
            var info = new FileInfo(p);
            sb.Append(p).Append('|').Append(info.Length).Append('|').Append(info.LastWriteTimeUtc.Ticks).Append(';');
        }
        var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(sb.ToString()));
        return Convert.ToHexString(bytes);
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
