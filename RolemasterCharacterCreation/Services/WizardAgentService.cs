using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using RolemasterCharacterCreation.Models;
using RolemasterCharacterCreation.Rules;

namespace RolemasterCharacterCreation.Services;

public class WizardAgentService(IHttpClientFactory httpFactory, IConfiguration config)
{
    const string ApiUrl = "https://api.anthropic.com/v1/messages";
    const string Model   = "claude-sonnet-4-6";

    const string SystemPrompt = """
        You are a concise expert on Rolemaster Unified (RMU) character creation rules.
        Answer the player's question given the current character state shown below.
        Be direct and specific — ideally under 150 words. Reference table numbers or
        rule names when helpful. If a rule is unclear or you're not certain, say so.
        Never invent rules.
        """;

    public async IAsyncEnumerable<string> AskAsync(
        string stepName,
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

        var userContent = BuildContext(stepName, character, stats) + "\n\nPlayer question: " + question;

        var body = new
        {
            model = Model,
            max_tokens = 512,
            stream = true,
            system = SystemPrompt,
            messages = new[] { new { role = "user", content = userContent } }
        };

        using var http = httpFactory.CreateClient("Anthropic");
        using var req  = new HttpRequestMessage(HttpMethod.Post, ApiUrl);
        req.Headers.Add("x-api-key", apiKey);
        req.Headers.Add("anthropic-version", "2023-06-01");
        req.Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

        using var resp = await http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct);

        if (!resp.IsSuccessStatusCode)
        {
            var err = await resp.Content.ReadAsStringAsync(ct);
            yield return $"⚠ API error {(int)resp.StatusCode}: {err}";
            yield break;
        }

        await using var stream = await resp.Content.ReadAsStreamAsync(ct);
        using var reader = new StreamReader(stream);

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

    static string BuildContext(string stepName, Character ch, IEnumerable<CharacterStat> stats)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Wizard step: {stepName}");
        sb.Append($"Character: {ch.Name}");
        if (ch.Race is not null)      sb.Append($", Race: {ch.Race}");
        if (ch.Profession is not null) sb.Append($", Profession: {ch.Profession}");
        if (ch.Culture is not null)    sb.Append($", Culture: {ch.Culture}");
        if (ch.Realm is not null)      sb.Append($", Realm: {ch.Realm}");
        sb.AppendLine($", Level: {ch.Level}");

        if (ch.Race is not null && RaceRules.ByName.TryGetValue(ch.Race, out var race))
        {
            sb.Append($"Race: bonus DP={race.BonusDP}, size={race.Size}, base hits={race.BaseHits}");
            var mods = Enum.GetValues<StatName>()
                .Where(sn => race.GetStatMod(sn) != 0)
                .Select(sn => $"{sn}{(race.GetStatMod(sn) > 0 ? "+" : "")}{race.GetStatMod(sn)}");
            var modStr = string.Join(" ", mods);
            if (modStr.Length > 0) sb.Append($", stat mods: {modStr}");
            sb.AppendLine();
        }

        var statList = stats.OrderBy(s => s.Stat).ToList();
        if (statList.Any())
        {
            var parts = statList.Select(s =>
                $"{s.Stat}={s.Temporary}({(SkillRules.StatBonus(s.Temporary) >= 0 ? "+" : "")}{SkillRules.StatBonus(s.Temporary)})");
            sb.AppendLine($"Stats (temp/bonus): {string.Join(" ", parts)}");
        }

        if (ch.RaceBonusDp > 0)
            sb.AppendLine($"DP budget: 60 + {ch.RaceBonusDp} (race) = {60 + ch.RaceBonusDp}");

        return sb.ToString().TrimEnd();
    }
}
