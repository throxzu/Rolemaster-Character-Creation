using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using RolemasterCharacterCreation.Models;

namespace RolemasterCharacterCreation.Services;

/// <summary>
/// Loads the curated misc reference tables (Core Law / Spell Law lookup tables)
/// from docs/game-data/reference-tables.json once at startup. Read-only; no database.
/// Mirrors <see cref="CreatureService"/>.
/// </summary>
public sealed class ReferenceTableService
{
    private static readonly JsonSerializerOptions JsonOpts =
        new() { PropertyNameCaseInsensitive = true };

    public IReadOnlyList<ReferenceTable> All { get; }
    public IReadOnlyList<string> Categories { get; }
    public IReadOnlyList<string> Books { get; }

    private readonly Dictionary<string, ReferenceTable> _byName;
    private readonly Dictionary<string, ReferenceTable> _bySkill;

    public ReferenceTableService(IWebHostEnvironment env)
    {
        var path = Path.GetFullPath(
            Path.Combine(DocsLocator.Root(env.ContentRootPath), "game-data/reference-tables.json"));

        All = File.Exists(path)
            ? JsonSerializer.Deserialize<List<ReferenceTable>>(File.ReadAllText(path), JsonOpts) ?? []
            : [];

        Categories = All.Select(t => t.Category).Distinct().Order().ToList();
        Books = All.Select(t => t.Book).Distinct().Order().ToList();

        _byName = All
            .GroupBy(t => t.Name, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

        _bySkill = new(StringComparer.OrdinalIgnoreCase);
        foreach (var t in All)
            foreach (var skill in t.Skills)
                _bySkill.TryAdd(skill, t);
    }

    /// <summary>The reference table whose <c>skills</c> list contains the given skill, or null.</summary>
    public ReferenceTable? TableForSkill(string skillName) =>
        _bySkill.GetValueOrDefault(skillName);

    public ReferenceTable? ByName(string name) => _byName.GetValueOrDefault(name);

    public IEnumerable<ReferenceTable> ByCategory(string category) =>
        All.Where(t => t.Category.Equals(category, StringComparison.OrdinalIgnoreCase));

    public IEnumerable<ReferenceTable> Search(string? term)
    {
        if (string.IsNullOrWhiteSpace(term)) return All;
        return All.Where(t =>
            t.Name.Contains(term, StringComparison.OrdinalIgnoreCase)
            || t.Number.Contains(term, StringComparison.OrdinalIgnoreCase)
            || t.Category.Contains(term, StringComparison.OrdinalIgnoreCase)
            || t.Book.Contains(term, StringComparison.OrdinalIgnoreCase));
    }
}
