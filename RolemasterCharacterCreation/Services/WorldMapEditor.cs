using System.Text.Json;
using System.Text.Json.Nodes;

namespace RolemasterCharacterCreation.Services;

/// <summary>
/// Mutations on a Watabou/Perilous Shores hex export. <see cref="WorldMapService"/> reads the
/// document and never writes it; this is the other half.
///
/// Nothing here needs the stable-id scheme the town editor has: a hex is identified by its
/// grid position, and a path by its key in a JSON object, so neither moves when something
/// beside it is added or removed. Everything the renderer ignores — <c>name</c>,
/// <c>origin</c>, <c>bp</c>, <c>layout</c>, and every hex not painted — is carried through
/// untouched.
/// </summary>
public class WorldMapEditor
{
    /// <summary>The three path collections, each keyed by a string in a JSON object.</summary>
    public const string Roads = "roads";
    public const string Rivers = "rivers";
    public const string SeaRoutes = "searoutes";

    public static bool IsPathLayer(string layer) =>
        layer is Roads or Rivers or SeaRoutes;

    /// <summary>
    /// Every terrain the renderer can draw. "plains" is the renderer's fallback rather than a
    /// value the generator writes, so painting it <em>removes</em> the property — see SetTerrain.
    /// </summary>
    public static readonly (string Value, string Label, string Fill)[] Palette =
    [
        ("water",        "Water",        "#a8c0d0"),
        ("plains",       "Plains",       "#f1ede1"),
        ("swamp",        "Swamp",        "#d4d4ba"),
        ("forest-light", "Light forest", "#e3e6d0"),
        ("forest-dark",  "Dark forest",  "#d7ddc0"),
        ("mountain",     "Mountain",     "#ece9e2"),
        ("rocks",        "Rocks",        "#e2ddd1"),
    ];

    public static string HexKey(int q, int r) => $"q{q}_r{r}";

    // ── Document access ──────────────────────────────────────────────────────

    public static JsonNode Parse(string rawJson) =>
        JsonNode.Parse(rawJson) ?? throw new InvalidOperationException("World map JSON is empty.");

    public static string Serialize(JsonNode doc) =>
        doc.ToJsonString(new JsonSerializerOptions { WriteIndented = true });

    private static JsonObject? Hexes(JsonNode doc) => doc["hexes"] as JsonObject;

    private static JsonObject? PathCollection(JsonNode doc, string layer, bool create)
    {
        if (!IsPathLayer(layer)) return null;
        if (doc[layer] is JsonObject existing) return existing;
        if (!create) return null;

        var added = new JsonObject();
        doc[layer] = added;
        return added;
    }

    // ── Operations ───────────────────────────────────────────────────────────
    //
    // Each returns the operation that undoes it, which is what the page's undo stack holds.

    /// <summary>One cell's terrain. Null means the hex carried no terrain at all, which the
    /// renderer draws as plains — restoring it has to remove the property again, not write
    /// "plains", or an undo would leave the file subtly different from how it started.</summary>
    public sealed record Cell(int Q, int R, string? Terrain);

    public sealed record Op(string Kind, string Layer, string? Key, Cell[]? Cells, string? Node);

    /// <summary>Repaints a set of hexes, skipping any the map does not contain.</summary>
    public static Op? SetTerrain(JsonNode doc, IEnumerable<(int Q, int R)> cells, string terrain)
    {
        if (Hexes(doc) is not { } hexes) return null;
        if (!Palette.Any(p => p.Value == terrain)) return null;

        var previous = new List<Cell>();
        foreach (var (q, r) in cells)
        {
            if (hexes[HexKey(q, r)] is not JsonObject hex) continue;   // outside the imported extent

            previous.Add(new Cell(q, r, hex["terrain"]?.GetValue<string>()));

            // The generator leaves plains unmarked, so painting plains means removing the key.
            if (terrain == "plains") hex.Remove("terrain");
            else hex["terrain"] = terrain;
        }

        return previous.Count == 0 ? null : new Op(nameof(RestoreTerrain), "hexes", null, [.. previous], null);
    }

    /// <summary>Puts each hex back exactly as it was, property-absence included.</summary>
    public static Op? RestoreTerrain(JsonNode doc, Cell[] cells)
    {
        if (Hexes(doc) is not { } hexes) return null;

        var previous = new List<Cell>();
        foreach (var c in cells)
        {
            if (hexes[HexKey(c.Q, c.R)] is not JsonObject hex) continue;

            previous.Add(new Cell(c.Q, c.R, hex["terrain"]?.GetValue<string>()));
            if (c.Terrain is null) hex.Remove("terrain");
            else hex["terrain"] = c.Terrain;
        }

        return previous.Count == 0 ? null : new Op(nameof(RestoreTerrain), "hexes", null, [.. previous], null);
    }

    /// <summary>
    /// Adds a path through these hexes. Roads and sea routes store the hexes themselves; a
    /// river stores the edges between consecutive hexes, because it is drawn along hex
    /// boundaries rather than through hex centres.
    /// </summary>
    public static Op? CreatePath(JsonNode doc, string layer, IReadOnlyList<(int Q, int R)> hexes, out string key)
    {
        key = "";
        if (PathCollection(doc, layer, create: true) is not { } coll) return null;
        if (hexes.Count < 2) return null;

        key = UniqueKey(coll, $"{HexKey(hexes[0].Q, hexes[0].R)}-{HexKey(hexes[^1].Q, hexes[^1].R)}");

        if (layer == Rivers)
        {
            var channel = new JsonArray();
            for (int i = 0; i + 1 < hexes.Count; i++)
                channel.Add($"{HexKey(hexes[i].Q, hexes[i].R)}:{HexKey(hexes[i + 1].Q, hexes[i + 1].R)}");
            coll[key] = new JsonObject { ["parent"] = null, ["channel"] = channel };
        }
        else
        {
            var chain = new JsonArray();
            foreach (var (q, r) in hexes) chain.Add(HexKey(q, r));
            coll[key] = chain;
        }

        return new Op(nameof(DeletePath), layer, key, null, null);
    }

    /// <summary>
    /// Removes a path, keeping its JSON verbatim so an undo restores it exactly — a river's
    /// <c>parent</c> links it to the watercourse it feeds, and rebuilding the node would lose it.
    /// </summary>
    public static Op? DeletePath(JsonNode doc, string layer, string key)
    {
        if (PathCollection(doc, layer, create: false) is not { } coll) return null;
        if (coll[key] is not { } node) return null;

        var verbatim = node.ToJsonString();
        coll.Remove(key);
        return new Op(nameof(RestorePath), layer, key, null, verbatim);
    }

    public static Op? RestorePath(JsonNode doc, string layer, string key, string node)
    {
        if (PathCollection(doc, layer, create: true) is not { } coll) return null;
        if (JsonNode.Parse(node) is not { } parsed) return null;

        coll[key] = parsed;
        return new Op(nameof(DeletePath), layer, key, null, null);
    }

    public static Op? Apply(JsonNode doc, Op op) => op.Kind switch
    {
        nameof(RestoreTerrain) when op.Cells is not null => RestoreTerrain(doc, op.Cells),
        nameof(DeletePath) when op.Key is not null => DeletePath(doc, op.Layer, op.Key),
        nameof(RestorePath) when op.Key is not null && op.Node is not null
            => RestorePath(doc, op.Layer, op.Key, op.Node),
        _ => null,
    };

    // A generated key can collide with one the generator already used — two roads between the
    // same pair of hexes is perfectly possible — and the second would silently replace the first.
    private static string UniqueKey(JsonObject coll, string wanted)
    {
        if (coll[wanted] is null) return wanted;
        for (int n = 2; ; n++)
        {
            var candidate = $"{wanted}#{n}";
            if (coll[candidate] is null) return candidate;
        }
    }
}
