using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using RolemasterCharacterCreation.Models;

namespace RolemasterCharacterCreation.Services;

/// <summary>
/// Loads the curated creature overview stat tables (Creature Law chapters 9–14)
/// from docs/game-data/creature-tables.json once at startup. Read-only; no database.
/// Mirrors <see cref="ReferenceTableService"/> but is a separate library so the two
/// datasets (and their pages) stay independent. These tables are GM-only.
/// </summary>
public sealed class CreatureTableService
{
    private static readonly JsonSerializerOptions JsonOpts =
        new() { PropertyNameCaseInsensitive = true };

    public IReadOnlyList<ReferenceTable> All { get; }
    public IReadOnlyList<string> Categories { get; }

    private readonly Dictionary<string, ReferenceTable> _byName;

    public CreatureTableService(IWebHostEnvironment env)
    {
        var path = Path.GetFullPath(
            Path.Combine(DocsLocator.Root(env.ContentRootPath), "game-data/creature-tables.json"));

        All = File.Exists(path)
            ? JsonSerializer.Deserialize<List<ReferenceTable>>(File.ReadAllText(path), JsonOpts) ?? []
            : [];

        Categories = All.Select(t => t.Category).Distinct().Order().ToList();

        _byName = All
            .GroupBy(t => t.Name, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);
    }

    public ReferenceTable? ByName(string name) => _byName.GetValueOrDefault(name);

    public IEnumerable<ReferenceTable> ByCategory(string category) =>
        All.Where(t => t.Category.Equals(category, StringComparison.OrdinalIgnoreCase));

    public IEnumerable<ReferenceTable> Search(string? term)
    {
        if (string.IsNullOrWhiteSpace(term)) return All;
        return All.Where(t =>
            t.Name.Contains(term, StringComparison.OrdinalIgnoreCase)
            || t.Category.Contains(term, StringComparison.OrdinalIgnoreCase)
            || t.Book.Contains(term, StringComparison.OrdinalIgnoreCase));
    }
}
