using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using RolemasterCharacterCreation.Models;
using RolemasterCharacterCreation.Rules;

namespace RolemasterCharacterCreation.Services;

public class CharacterSheetAgentService(IHttpClientFactory httpFactory, IConfiguration config)
{
    const string ApiUrl = "https://api.anthropic.com/v1/messages";
    const string Model   = "claude-sonnet-4-6";

    const string AskSystemPrompt = """
        You are an expert on the Rolemaster Unified (RMU) character sheet layout and what
        each field means. Answer questions about how a character's data is calculated and
        displayed on the sheet — stat bonuses, resistance rolls, skill totals, derived values.
        Be concise (under 150 words). Reference specific table numbers when helpful.
        Never invent rules; if uncertain, say so.
        """;

    const string ValidateSystemPrompt = """
        You are an RMU rules validator. Given a character's current data, identify any
        rule violations or inconsistencies that would cause incorrect values on the
        character sheet. Focus on: stat values out of range, missing required fields
        (race, profession, culture), DP budget exceeded, resistance roll stat assignments,
        and racial modifier application. List each issue in one short sentence.
        If everything is consistent, respond with exactly: OK
        """;

    // ── Streaming Q&A for the sheet Oracle panel ─────────────────────────────

    public async IAsyncEnumerable<string> AskAsync(
        Character character,
        IEnumerable<CharacterStat> stats,
        string question,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        var apiKey = config["Anthropic:ApiKey"] ?? "";
        if (string.IsNullOrEmpty(apiKey))
        {
            yield return "⚠ No API key configured. Add `Anthropic:ApiKey` to appsettings.Development.json.";
            yield break;
        }

        var userContent = BuildContext(character, stats) + "\n\nQuestion: " + question;

        await foreach (var token in StreamAsync(AskSystemPrompt, userContent, 512, apiKey, ct))
            yield return token;
    }

    // ── One-shot validation called by the wizard after saving a step ──────────

    public async Task<string?> ValidateAsync(
        Character character,
        IEnumerable<CharacterStat> stats,
        CancellationToken ct = default)
    {
        var apiKey = config["Anthropic:ApiKey"] ?? "";
        if (string.IsNullOrEmpty(apiKey)) return null; // silently skip if not configured

        // Quick programmatic pre-checks — avoid a round-trip for obvious cases
        var issues = new List<string>();
        if (string.IsNullOrEmpty(character.Race))       issues.Add("Race is not set.");
        if (string.IsNullOrEmpty(character.Profession)) issues.Add("Profession is not set.");
        if (string.IsNullOrEmpty(character.Culture))    issues.Add("Culture is not set.");

        var statList = stats.ToList();
        foreach (var s in statList)
        {
            if (s.Temporary is < 1 or > 100)
                issues.Add($"{s.Stat} temporary value {s.Temporary} is outside 1–100.");
            if (s.Potential < s.Temporary)
                issues.Add($"{s.Stat} potential ({s.Potential}) is less than temporary ({s.Temporary}).");
        }

        // If obvious issues were found, return them without calling the API
        if (issues.Any()) return string.Join("\n", issues);

        // Ask Claude for a holistic check
        var userContent = "Validate the following character for rules compliance:\n\n"
                          + BuildContext(character, statList);

        var sb = new StringBuilder();
        await foreach (var token in StreamAsync(ValidateSystemPrompt, userContent, 256, apiKey, ct))
            sb.Append(token);

        var result = sb.ToString().Trim();
        return result == "OK" ? null : result;
    }

    // ── Shared SSE streaming helper ───────────────────────────────────────────

    private async IAsyncEnumerable<string> StreamAsync(
        string systemPrompt, string userContent, int maxTokens, string apiKey,
        [EnumeratorCancellation] CancellationToken ct)
    {
        var body = new
        {
            model = Model,
            max_tokens = maxTokens,
            stream = true,
            system = systemPrompt,
            messages = new[] { new { role = "user", content = userContent } }
        };

        using var http = httpFactory.CreateClient("Anthropic");
        using var req  = new HttpRequestMessage(HttpMethod.Post, ApiUrl);
        req.Headers.Add("x-api-key", apiKey);
        req.Headers.Add("anthropic-version", "2023-06-01");
        req.Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

        HttpResponseMessage resp;
        try { resp = await http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct); }
        catch (OperationCanceledException) { yield break; }

        if (!resp.IsSuccessStatusCode)
        {
            var err = await resp.Content.ReadAsStringAsync(ct);
            yield return $"⚠ API error {(int)resp.StatusCode}: {err}";
            resp.Dispose();
            yield break;
        }

        await using var stream = await resp.Content.ReadAsStreamAsync(ct);
        using var reader = new StreamReader(stream);
        resp.Dispose();

        while (!reader.EndOfStream && !ct.IsCancellationRequested)
        {
            var line = await reader.ReadLineAsync(ct);
            if (line is null || !line.StartsWith("data: ")) continue;
            var data = line[6..];
            if (data == "[DONE]") break;

            string? token = null;
            try
            {
                using var doc = JsonDocument.Parse(data);
                if (doc.RootElement.TryGetProperty("type", out var typeEl)
                    && typeEl.GetString() == "content_block_delta"
                    && doc.RootElement.TryGetProperty("delta", out var delta)
                    && delta.TryGetProperty("text", out var text))
                {
                    token = text.GetString();
                }
            }
            catch { /* skip malformed SSE lines */ }

            if (token is not null) yield return token;
        }
    }

    // ── Context builder shared by both methods ────────────────────────────────

    static string BuildContext(Character ch, IEnumerable<CharacterStat> stats)
    {
        var sb = new StringBuilder();
        sb.Append($"Character: {ch.Name}");
        if (ch.Race is not null)       sb.Append($", Race: {ch.Race}");
        if (ch.Profession is not null)  sb.Append($", Profession: {ch.Profession}");
        if (ch.Culture is not null)     sb.Append($", Culture: {ch.Culture}");
        if (ch.Realm is not null)       sb.Append($", Realm: {ch.Realm}");
        sb.AppendLine($", Level: {ch.Level}");

        if (ch.Race is not null && RaceRules.ByName.TryGetValue(ch.Race, out var race))
        {
            sb.Append($"Race data: bonus DP={race.BonusDP}, size={race.Size}, base hits={race.BaseHits}");
            var mods = Enum.GetValues<StatName>()
                .Where(sn => race.GetStatMod(sn) != 0)
                .Select(sn => $"{sn}{(race.GetStatMod(sn) > 0 ? "+" : "")}{race.GetStatMod(sn)}");
            var rrs = new[]
            {
                $"Ch{(race.ResistChanneling >= 0 ? "+" : "")}{race.ResistChanneling}",
                $"Es{(race.ResistEssence    >= 0 ? "+" : "")}{race.ResistEssence}",
                $"Mn{(race.ResistMentalism  >= 0 ? "+" : "")}{race.ResistMentalism}",
                $"Ph{(race.ResistPhysical   >= 0 ? "+" : "")}{race.ResistPhysical}",
            };
            sb.AppendLine($", stat mods: {string.Join(" ", mods)}, RR mods: {string.Join(" ", rrs)}");
        }

        var statList = stats.OrderBy(s => s.Stat).ToList();
        if (statList.Any())
        {
            var parts = statList.Select(s =>
            {
                int racial = ch.Race is not null && RaceRules.ByName.TryGetValue(ch.Race, out var r)
                    ? r.GetStatMod(s.Stat) : 0;
                int eff = SkillRules.StatBonus(s.Temporary) + racial;
                return $"{s.Stat}={s.Temporary}/pot={s.Potential} eff={eff:+#;-#;0}";
            });
            sb.AppendLine($"Stats (temp/potential/effective bonus): {string.Join("  ", parts)}");
        }

        if (ch.Skills.Any())
        {
            var skillSummary = ch.Skills
                .Where(s => s.TotalRanks > 0 || s.IsProfessionalSkill)
                .OrderBy(s => s.Category).ThenBy(s => s.SkillName)
                .Select(s => $"{s.SkillName}:{s.TotalRanks}r{(s.IsProfessionalSkill ? " Pro" : "")}{(s.IsKnack ? " Knack" : "")}");
            sb.AppendLine($"Skills: {string.Join(", ", skillSummary)}");
        }

        if (ch.RaceBonusDp > 0)
            sb.AppendLine($"DP budget: {60 + ch.RaceBonusDp} (60 base + {ch.RaceBonusDp} race)");

        return sb.ToString().TrimEnd();
    }
}
