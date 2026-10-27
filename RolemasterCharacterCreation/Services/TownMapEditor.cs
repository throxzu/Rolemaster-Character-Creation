using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace RolemasterCharacterCreation.Services;

/// <summary>
/// Mutations on a Watabou/MFCG town export. <see cref="TownMapService"/> reads the document
/// and never writes it; this is the other half — a mutable round trip that lets the GM
/// reshape, add and delete buildings, walls and roads.
///
/// Two rules govern everything here. Anything the renderer ignores is carried through
/// untouched, so a saved map keeps the generator's own metadata and the terrain it drew. And
/// every feature carries a <em>stable id</em> rather than being identified by its position:
/// a place mark used to point at "building 72", which stopped meaning the same building the
/// moment building 12 was deleted.
/// </summary>
public class TownMapEditor
{
    /// <summary>The id array written beside a layer's geometry. Namespaced so it cannot collide
    /// with anything the generator emits, and ignored by any other GeoJSON reader.</summary>
    public const string IdsProperty = "rmIds";

    /// <summary>Next id to hand out for a layer, so a deleted id is never reused.</summary>
    public const string NextIdProperty = "rmNextId";

    /// <summary>Layers held as a MultiPolygon in <c>coordinates</c>.</summary>
    private static readonly string[] PolygonLayers = ["buildings", "squares", "prisms"];

    /// <summary>Layers held as a GeometryCollection in <c>geometries</c>.</summary>
    private static readonly string[] CollectionLayers = ["walls", "roads", "planks"];

    public static bool IsEditableLayer(string layer) =>
        PolygonLayers.Contains(layer) || CollectionLayers.Contains(layer);

    // ── Document access ──────────────────────────────────────────────────────

    public static JsonNode Parse(string rawJson) =>
        JsonNode.Parse(rawJson) ?? throw new InvalidOperationException("Town map JSON is empty.");

    public static string Serialize(JsonNode doc) =>
        doc.ToJsonString(new JsonSerializerOptions { WriteIndented = true });

    private static JsonArray? Features(JsonNode doc) => doc["features"] as JsonArray;

    /// <summary>The feature object for a layer, e.g. the one whose <c>id</c> is "buildings".</summary>
    private static JsonObject? Layer(JsonNode doc, string layer) =>
        Features(doc)?.OfType<JsonObject>()
            .FirstOrDefault(f => f["id"]?.GetValue<string>() == layer);

    /// <summary>The array a layer's features live in: polygons, or geometries.</summary>
    private static JsonArray? Items(JsonObject layerObj) =>
        layerObj["coordinates"] as JsonArray ?? layerObj["geometries"] as JsonArray;

    // ── Stable ids ───────────────────────────────────────────────────────────

    /// <summary>
    /// Makes sure every editable layer carries an id per feature. Ids are handed out in the
    /// array's current order, so on a town that has never been edited an id equals its index —
    /// which is what makes backfilling the existing place marks from their old index safe.
    /// Returns true when the document was changed and needs saving.
    /// </summary>
    public static bool EnsureFeatureIds(JsonNode doc)
    {
        bool changed = false;

        foreach (var layer in PolygonLayers.Concat(CollectionLayers))
        {
            if (Layer(doc, layer) is not { } layerObj) continue;
            if (Items(layerObj) is not { } items) continue;
            if (layerObj[IdsProperty] is JsonArray existing && existing.Count == items.Count) continue;

            var ids = new JsonArray();
            for (int i = 0; i < items.Count; i++) ids.Add(i);
            layerObj[IdsProperty] = ids;
            layerObj[NextIdProperty] = items.Count;
            changed = true;
        }

        return changed;
    }

    /// <summary>Position of a feature within its layer, or −1 if that id is not there.</summary>
    public static int IndexOfId(JsonNode doc, string layer, int id)
    {
        if (Layer(doc, layer) is not { } layerObj) return -1;
        if (layerObj[IdsProperty] is not JsonArray ids) return -1;

        for (int i = 0; i < ids.Count; i++)
            if (ids[i]?.GetValue<int>() == id) return i;

        return -1;
    }

    private static int TakeNextId(JsonObject layerObj)
    {
        int next = layerObj[NextIdProperty]?.GetValue<int>() ?? 0;
        layerObj[NextIdProperty] = next + 1;
        return next;
    }

    // ── Operations ───────────────────────────────────────────────────────────
    //
    // Each returns the operation that undoes it, which is what the page's undo stack holds.

    /// <summary>One reversible change to the map.</summary>
    public sealed record Op(string Kind, string Layer, int Id, double[][]? Points, int? AtIndex);

    /// <summary>Replaces a feature's outline. Used for both dragging a vertex and moving the whole shape.</summary>
    public static Op? SetPoints(JsonNode doc, string layer, int id, double[][] points)
    {
        if (!IsEditableLayer(layer)) return null;
        if (Layer(doc, layer) is not { } layerObj) return null;
        if (Items(layerObj) is not { } items) return null;

        int i = IndexOfId(doc, layer, id);
        if (i < 0 || i >= items.Count) return null;

        var before = ReadPoints(items[i], layer);
        if (before is null) return null;

        WritePoints(items, i, layer, points);
        return new Op(nameof(SetPoints), layer, id, before, null);
    }

    /// <summary>Adds a building and returns its new id through <paramref name="newId"/>.</summary>
    public static Op? CreateFeature(JsonNode doc, string layer, double[][] points, out int newId)
    {
        newId = -1;
        if (!IsEditableLayer(layer)) return null;
        if (Layer(doc, layer) is not { } layerObj) return null;
        if (Items(layerObj) is not { } items) return null;
        if (layerObj[IdsProperty] is not JsonArray ids) return null;

        newId = TakeNextId(layerObj);
        items.Add(NewItem(layer, points));
        ids.Add(newId);

        return new Op(nameof(DeleteFeature), layer, newId, null, null);
    }

    /// <summary>
    /// Removes a feature. The id is retired rather than reused, so a mark that still points at
    /// it resolves to nothing instead of silently adopting some other building.
    /// </summary>
    public static Op? DeleteFeature(JsonNode doc, string layer, int id)
    {
        if (!IsEditableLayer(layer)) return null;
        if (Layer(doc, layer) is not { } layerObj) return null;
        if (Items(layerObj) is not { } items) return null;
        if (layerObj[IdsProperty] is not JsonArray ids) return null;

        int i = IndexOfId(doc, layer, id);
        if (i < 0 || i >= items.Count) return null;

        var points = ReadPoints(items[i], layer);
        items.RemoveAt(i);
        ids.RemoveAt(i);

        // Restoring puts it back where it was, so an undo does not shuffle the drawing order.
        return points is null ? null : new Op(nameof(RestoreFeature), layer, id, points, i);
    }

    /// <summary>Puts a deleted feature back, with its original id and position.</summary>
    public static Op? RestoreFeature(JsonNode doc, string layer, int id, double[][] points, int atIndex)
    {
        if (!IsEditableLayer(layer)) return null;
        if (Layer(doc, layer) is not { } layerObj) return null;
        if (Items(layerObj) is not { } items) return null;
        if (layerObj[IdsProperty] is not JsonArray ids) return null;

        int at = Math.Clamp(atIndex, 0, items.Count);
        items.Insert(at, NewItem(layer, points));
        ids.Insert(at, id);

        return new Op(nameof(DeleteFeature), layer, id, null, null);
    }

    /// <summary>Applies an operation and returns the one that undoes it.</summary>
    public static Op? Apply(JsonNode doc, Op op) => op.Kind switch
    {
        nameof(SetPoints) when op.Points is not null => SetPoints(doc, op.Layer, op.Id, op.Points),
        nameof(DeleteFeature) => DeleteFeature(doc, op.Layer, op.Id),
        nameof(RestoreFeature) when op.Points is not null && op.AtIndex is not null
            => RestoreFeature(doc, op.Layer, op.Id, op.Points, op.AtIndex.Value),
        _ => null,
    };

    // ── Geometry shaping ─────────────────────────────────────────────────────

    /// <summary>
    /// A polygon layer stores <c>[[ [x,y], ... ]]</c> — a ring inside a polygon. A geometry
    /// collection stores a geometry object whose coordinates are either a bare LineString or,
    /// for walls, a ring nested one deeper. This hides that difference from the callers.
    /// </summary>
    private static double[][]? ReadPoints(JsonNode? item, string layer)
    {
        if (item is null) return null;

        var coords = PolygonLayers.Contains(layer)
            ? item as JsonArray                      // the polygon: [ring, ...]
            : item["coordinates"] as JsonArray;      // the geometry's own coordinates

        if (coords is null || coords.Count == 0) return null;

        // Ring-in-polygon (walls, buildings) versus a flat LineString (roads).
        var ring = coords[0] is JsonArray first && first.Count > 0 && first[0] is JsonArray
            ? first
            : coords;

        var result = new List<double[]>();
        foreach (var pair in ring.OfType<JsonArray>())
        {
            if (pair.Count < 2) continue;
            result.Add([pair[0]!.GetValue<double>(), pair[1]!.GetValue<double>()]);
        }
        return result.Count == 0 ? null : [.. result];
    }

    private static void WritePoints(JsonArray items, int index, string layer, double[][] points)
    {
        if (PolygonLayers.Contains(layer))
        {
            items[index] = RingInPolygon(points);
            return;
        }

        // Keep the geometry object — it carries the stroke width and, for a road, its name.
        if (items[index] is not JsonObject geom) { items[index] = NewItem(layer, points); return; }

        bool nested = geom["coordinates"] is JsonArray c && c.Count > 0
                      && c[0] is JsonArray f && f.Count > 0 && f[0] is JsonArray;
        geom["coordinates"] = nested ? RingInPolygon(points) : Ring(points);
    }

    private static JsonNode NewItem(string layer, double[][] points) =>
        PolygonLayers.Contains(layer)
            ? RingInPolygon(points)
            : new JsonObject { ["type"] = "LineString", ["coordinates"] = Ring(points) };

    private static JsonArray Ring(double[][] points)
    {
        var ring = new JsonArray();
        foreach (var p in points)
            ring.Add(new JsonArray(Round(p[0]), Round(p[1])));
        return ring;
    }

    private static JsonArray RingInPolygon(double[][] points) => new(Ring(points));

    // The generator writes three decimals; matching it keeps an edited file looking like the
    // rest of itself and stops a drag from storing seventeen digits of floating-point noise.
    private static JsonNode Round(double v) =>
        JsonValue.Create(Math.Round(v, 3, MidpointRounding.AwayFromZero))!;

    /// <summary>Formats a point list the way the renderer's SVG "points" attribute wants it.</summary>
    public static string ToSvgPoints(double[][] points) =>
        string.Join(' ', points.Select(p =>
            string.Create(CultureInfo.InvariantCulture, $"{p[0]:0.##},{p[1]:0.##}")));
}
