using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Hosting;
using RolemasterCharacterCreation.Models;

namespace RolemasterCharacterCreation.Services;

/// <summary>
/// Loads the Treasure Law chapter 6 named magic items from
/// docs/game-data/magic-items.json once at startup, plus any GM-created (LLM-generated)
/// items from docs/game-data/created-magic-items.json. Read-only at the page level except
/// for <see cref="AddCreatedAsync"/>, which appends a generated item. No database.
/// Gamemaster-only data. Mirrors <see cref="ReferenceTableService"/>.
/// </summary>
public sealed class MagicItemService
{
    private static readonly JsonSerializerOptions JsonOpts =
        new() { PropertyNameCaseInsensitive = true };
    private static readonly JsonSerializerOptions WriteOpts =
        new() { WriteIndented = true };

    private readonly string _createdPath;
    private readonly object _gate = new();

    private readonly List<MagicItem> _rulebook;
    private readonly List<MagicItem> _created;
    private List<MagicItem> _all;
    private Dictionary<string, MagicItem> _byNorm;

    public IReadOnlyList<MagicItem> All { get { lock (_gate) return _all; } }
    public IReadOnlyList<string> Categories
    {
        get { lock (_gate) return _all.Select(t => t.Category).Distinct().ToList(); }
    }

    public MagicItemService(IWebHostEnvironment env)
    {
        var root = DocsLocator.Root(env.ContentRootPath);
        var path = Path.GetFullPath(Path.Combine(root, "game-data/magic-items.json"));
        _createdPath = Path.GetFullPath(Path.Combine(root, "game-data/created-magic-items.json"));

        _rulebook = Load(path);
        foreach (var m in _rulebook) m.Created = false;

        _created = Load(_createdPath);
        foreach (var m in _created) m.Created = true;

        Rebuild();
    }

    private static List<MagicItem> Load(string path) =>
        File.Exists(path)
            ? JsonSerializer.Deserialize<List<MagicItem>>(File.ReadAllText(path), JsonOpts) ?? []
            : [];

    // Recompute the merged view and name index. Caller holds the lock (or is the ctor).
    private void Rebuild()
    {
        _all = _rulebook.Concat(_created).ToList();
        _byNorm = new(StringComparer.Ordinal);
        foreach (var m in _all)
            _byNorm.TryAdd(Norm(m.Name), m);
    }

    // Normalise for matching: unify curly/straight apostrophes, collapse whitespace, lowercase.
    private static string Norm(string s) =>
        Regex.Replace(s.Replace('’', '\'').Replace('‘', '\''), @"\s+", " ")
             .Trim().ToLowerInvariant();

    // Strip a trailing variant ("…: Lesser") or component/qualifier ("… (full suit)") suffix
    // so e.g. "Blade of Frost: Lesser" / "Armor of Golspre (full suit)" match the base item.
    private static string StripVariant(string s)
    {
        s = Regex.Replace(s, @":\s*(Lesser|Greater|Minor|Moderate|Major)\b.*$", "",
                          RegexOptions.IgnoreCase);
        s = Regex.Replace(s, @"\s*\([^)]*\)\s*$", "");
        return s.Trim();
    }

    /// <summary>
    /// Resolve free text (e.g. a Random Treasure table cell) to a magic item, or null.
    /// Tries an exact normalised match, then falls back to the base name without a
    /// variant/qualifier suffix.
    /// </summary>
    public MagicItem? Resolve(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return null;
        lock (_gate)
        {
            if (_byNorm.TryGetValue(Norm(text), out var m)) return m;
            return _byNorm.TryGetValue(Norm(StripVariant(text)), out m) ? m : null;
        }
    }

    public IEnumerable<MagicItem> Search(string? term)
    {
        var all = All;
        if (string.IsNullOrWhiteSpace(term)) return all;
        return all.Where(i =>
            i.Name.Contains(term, StringComparison.OrdinalIgnoreCase)
            || i.Category.Contains(term, StringComparison.OrdinalIgnoreCase)
            || i.Description.Contains(term, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Append a GM-generated item to the catalog: persists it to created-magic-items.json and
    /// merges it into the in-memory view so it appears immediately. If the name collides with an
    /// existing item, a numeric suffix is added. Returns the stored item.
    /// </summary>
    public async Task<MagicItem> AddCreatedAsync(MagicItem item)
    {
        item.Created = true;
        lock (_gate)
        {
            item.Name = UniqueName(item.Name);
            _created.Add(item);
            Rebuild();
        }
        await PersistCreatedAsync();
        return item;
    }

    // Caller holds the lock.
    private string UniqueName(string name)
    {
        var existing = _all.Select(m => m.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
        if (!existing.Contains(name)) return name;
        for (var n = 2; ; n++)
        {
            var candidate = $"{name} ({n})";
            if (!existing.Contains(candidate)) return candidate;
        }
    }

    private async Task PersistCreatedAsync()
    {
        List<MagicItem> snapshot;
        lock (_gate) snapshot = _created.ToList();
        var json = JsonSerializer.Serialize(snapshot, WriteOpts);
        await File.WriteAllTextAsync(_createdPath, json);
    }
}
