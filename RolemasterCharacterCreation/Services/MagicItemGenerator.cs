using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Anthropic;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using RolemasterCharacterCreation.Models;
using ChatMessage = Microsoft.Extensions.AI.ChatMessage;

namespace RolemasterCharacterCreation.Services;

/// <summary>
/// Generates a single Rolemaster (RMU) magic item from GM-chosen parameters via Claude.
/// Mirrors the LLM wiring in <see cref="RulesIndexService"/> (AnthropicClient -&gt; IChatClient).
/// The model is asked for a strict JSON object so the result maps onto <see cref="MagicItem"/>.
/// </summary>
public sealed class MagicItemGenerator
{
    private readonly IChatClient _chat;
    private readonly MagicItemService _items;
    private readonly IConfiguration _config;

    public MagicItemGenerator(AnthropicClient anthropic, IOptions<RulesAssistantOptions> options,
                              MagicItemService items, IConfiguration config)
    {
        _chat = anthropic.AsIChatClient(options.Value.ChatModel);
        _items = items;
        _config = config;
    }

    /// <summary>Whether the LLM is configured (API key present).</summary>
    public bool IsAvailable => !string.IsNullOrWhiteSpace(_config["ANTHROPIC_API_KEY"]);

    private sealed record ItemDto(string? Name, string? Description, string? Recipe,
                                  string? Level, string? Cost, string? Days);

    /// <summary>
    /// Generate an item for the given type/category, optional subtype, estimated level and free-text
    /// guidance. Returns an unsaved <see cref="MagicItem"/> tagged Created, with Category set to
    /// <paramref name="category"/>.
    /// </summary>
    public async Task<MagicItem> GenerateAsync(string typeLabel, string category, string? subtype,
                                               int level, string? guidance, CancellationToken ct)
    {
        if (!IsAvailable)
            throw new InvalidOperationException(
                "Magic item generation is disabled — set ANTHROPIC_API_KEY.");

        var system = BuildSystemPrompt(category);
        var user = BuildUserPrompt(typeLabel, subtype, level, guidance);

        var messages = new List<ChatMessage>
        {
            new(ChatRole.System, system),
            new(ChatRole.User, user),
        };
        var response = await _chat.GetResponseAsync(messages, new ChatOptions { MaxOutputTokens = 1500 }, ct);

        var dto = ParseJson(response.Text)
                  ?? throw new InvalidOperationException("The model did not return a valid item. Try again.");

        return new MagicItem
        {
            Name = (dto.Name ?? "Unnamed Item").Trim(),
            Category = category,
            Description = (dto.Description ?? "").Trim(),
            Recipe = (dto.Recipe ?? "").Trim(),
            Level = string.IsNullOrWhiteSpace(dto.Level) ? level.ToString() : dto.Level.Trim(),
            Cost = (dto.Cost ?? "").Trim(),
            Days = (dto.Days ?? "").Trim(),
            Created = true,
        };
    }

    private string BuildSystemPrompt(string category)
    {
        // Ground format & pricing on a couple of real rulebook items from the same category.
        var examples = _items.All
            .Where(i => i.Category == category && !i.Created && !string.IsNullOrWhiteSpace(i.Recipe))
            .Take(2).ToList();

        var sb = new StringBuilder();
        sb.AppendLine(
            "You are a Rolemaster Unified (RMU) Treasure Law magic-item designer. Invent ONE balanced, " +
            "evocative magic item matching the requested type, subtype and approximate item level. Keep it " +
            "consistent with RMU conventions:");
        sb.AppendLine("- The enchantment 'recipe' lists the Work spell and each embedded/daily/constant/" +
            "spell with its level in parentheses, then 'Days: N' and 'Standard cost: X sp', then 'Level N item.'");
        sb.AppendLine("- Powers should be appropriate to the item level (higher level = more/stronger powers).");
        sb.AppendLine("- The item level is the level of the highest-level spell used.");
        if (examples.Count > 0)
        {
            sb.AppendLine("\nExample items from this category (for format and pricing — do NOT copy them):");
            foreach (var e in examples)
                sb.AppendLine($"- {e.Name}: {e.Description} [Recipe: {e.Recipe}]");
        }
        sb.AppendLine(
            "\nReturn ONLY a JSON object (no markdown, no commentary) with these string keys: " +
            "\"name\", \"description\", \"recipe\", \"level\", \"cost\", \"days\". " +
            "\"level\" is the numeric item level, \"cost\" like \"1,234 sp\", \"days\" the working days as a number.");
        return sb.ToString();
    }

    private static string BuildUserPrompt(string typeLabel, string? subtype, int level, string? guidance)
    {
        var sb = new StringBuilder();
        sb.Append("Create a magic item. Type: ").Append(typeLabel).Append('.');
        if (!string.IsNullOrWhiteSpace(subtype))
            sb.Append(" Subtype: ").Append(subtype).Append('.');
        sb.Append(" Approximate item level: ").Append(level).Append('.');
        if (!string.IsNullOrWhiteSpace(guidance))
            sb.Append(" GM guidance: ").Append(guidance.Trim());
        return sb.ToString();
    }

    private static ItemDto? ParseJson(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return null;
        // Strip ```json fences and isolate the first {...} block.
        var t = text.Trim();
        var match = Regex.Match(t, @"\{.*\}", RegexOptions.Singleline);
        if (!match.Success) return null;
        try
        {
            return JsonSerializer.Deserialize<ItemDto>(match.Value,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        catch (JsonException)
        {
            return null;
        }
    }
}
