using System.Globalization;
using System.Text;
using System.Text.Json;

namespace RolemasterCharacterCreation.Services;

// Parses a Watabou/MFCG city export (GeoJSON-style FeatureCollection) into a flat,
// render-ready model for inline SVG. Stateless and thread-safe (registered singleton).
public class TownMapService
{
    // A closed polygon outline (outer ring only) as an SVG "points" string.
    public sealed record Poly(string Points);

    // An open or closed line drawn as a stroke, with its world-space width.
    public sealed record Line(string Points, double Width);

    // A named district region (subtle background fill) plus the centroid where its
    // name label is drawn.
    public sealed record District(string Points, string? Name, double Cx, double Cy);

    // A clickable feature; Index is its position within its layer's coordinate array.
    public sealed record Feature(int Index, string Points);

    public sealed class TownRender
    {
        public double MinX { get; set; }
        public double MinY { get; set; }
        public double Width { get; set; } = 100;
        public double Height { get; set; } = 100;

        public string ViewBox => string.Create(CultureInfo.InvariantCulture,
            $"{MinX:0.##} {MinY:0.##} {Width:0.##} {Height:0.##}");

        public List<Poly> Water { get; } = [];
        public List<Poly> Earth { get; } = [];
        // Agricultural plots surrounding a settlement (MFCG "fields"). Drawn behind the
        // built-up area; excluded from the bounding box like earth so they don't shrink
        // the frame. Rendered by the village map for its hand-drawn farmland look.
        public List<Poly> Fields { get; } = [];
        public List<District> Districts { get; } = [];
        public List<Line> Roads { get; } = [];
        public List<Line> Planks { get; } = [];
        public List<Line> Walls { get; } = [];
        public List<Feature> Squares { get; } = [];
        public List<Feature> Buildings { get; } = [];
        public List<Feature> Prisms { get; } = [];
    }

    // Quick sanity check used by the upload page before persisting an unknown file.
    public bool IsValid(string rawJson, out string? error)
    {
        error = null;
        try
        {
            using var doc = JsonDocument.Parse(rawJson);
            var root = doc.RootElement;
            if (root.ValueKind != JsonValueKind.Object ||
                !root.TryGetProperty("type", out var type) ||
                type.GetString() != "FeatureCollection")
            {
                error = "Not a city map: expected a GeoJSON FeatureCollection.";
                return false;
            }
            if (!root.TryGetProperty("features", out var features) ||
                features.ValueKind != JsonValueKind.Array ||
                features.GetArrayLength() == 0)
            {
                error = "City map has no features.";
                return false;
            }
            return true;
        }
        catch (JsonException ex)
        {
            error = $"Invalid JSON: {ex.Message}";
            return false;
        }
    }

    public TownRender Build(string rawJson)
    {
        var render = new TownRender();
        // Bounding box accumulated from the land + city content only (the surrounding
        // sea is far larger and would dwarf the city, so water is excluded and clipped).
        double minX = double.MaxValue, minY = double.MaxValue;
        double maxX = double.MinValue, maxY = double.MinValue;
        bool any = false;

        void Track(double x, double y)
        {
            any = true;
            if (x < minX) minX = x;
            if (y < minY) minY = y;
            if (x > maxX) maxX = x;
            if (y > maxY) maxY = y;
        }

        using var doc = JsonDocument.Parse(rawJson);
        var root = doc.RootElement;
        if (!root.TryGetProperty("features", out var features) ||
            features.ValueKind != JsonValueKind.Array)
        {
            return render;
        }

        foreach (var f in features.EnumerateArray())
        {
            var id = f.TryGetProperty("id", out var idEl) ? idEl.GetString() : null;
            switch (id)
            {
                case "water":
                    foreach (var poly in OuterRings(f, "coordinates", multi: true))
                        render.Water.Add(new Poly(Ring(poly, track: false, Track)));
                    break;

                // Earth/roads/planks/districts are drawn but NOT used for the bounding
                // box: they extend far beyond the built-up town (the whole island, roads
                // trailing off into the countryside) and would shrink the town to a dot.
                // The viewBox is framed on the town structures only (walls + buildings +
                // squares + prisms); everything else simply clips at the frame edge.
                case "earth":
                    foreach (var poly in OuterRings(f, "coordinates", multi: false))
                        render.Earth.Add(new Poly(Ring(poly, track: false, Track)));
                    break;

                // Farm fields: laid out either as a GeometryCollection (like districts)
                // or as a feature with its own MultiPolygon coordinates. Handle both, and
                // keep them out of the bounding box so they frame loosely behind the town.
                case "fields":
                    if (f.TryGetProperty("geometries", out _))
                    {
                        foreach (var (geom, _) in GeometryCollection(f))
                            foreach (var ring in OuterRings(geom, "coordinates", multi: false))
                                render.Fields.Add(new Poly(Ring(ring, track: false, Track)));
                    }
                    else
                    {
                        foreach (var poly in OuterRings(f, "coordinates", multi: true))
                            render.Fields.Add(new Poly(Ring(poly, track: false, Track)));
                    }
                    break;

                case "roads":
                    foreach (var (geom, _) in GeometryCollection(f))
                        render.Roads.Add(new Line(LineString(geom, track: false, Track), Width(geom, 8)));
                    break;

                case "planks":
                    foreach (var (geom, _) in GeometryCollection(f))
                        render.Planks.Add(new Line(LineString(geom, track: false, Track), Width(geom, 4)));
                    break;

                case "walls":
                    foreach (var (geom, _) in GeometryCollection(f))
                        foreach (var ring in OuterRings(geom, "coordinates", multi: false))
                            render.Walls.Add(new Line(Ring(ring, track: true, Track), Width(geom, 7)));
                    break;

                case "districts":
                    foreach (var (geom, name) in GeometryCollection(f))
                        foreach (var ring in OuterRings(geom, "coordinates", multi: false))
                        {
                            var (cx, cy) = Centroid(ring);
                            render.Districts.Add(new District(Ring(ring, track: false, Track), name, cx, cy));
                        }
                    break;

                case "squares":
                    AddFeatures(f, render.Squares, Track);
                    break;

                case "buildings":
                    AddFeatures(f, render.Buildings, Track);
                    break;

                case "prisms":
                    AddFeatures(f, render.Prisms, Track);
                    break;
            }
        }

        if (any)
        {
            double padX = (maxX - minX) * 0.04;
            double padY = (maxY - minY) * 0.04;
            render.MinX = minX - padX;
            render.MinY = minY - padY;
            render.Width = (maxX - minX) + padX * 2;
            render.Height = (maxY - minY) + padY * 2;
        }

        return render;
    }

    // Adds each top-level polygon of a MultiPolygon as a clickable feature, preserving
    // its index so a click maps back to TownLocation.FeatureIndex.
    private static void AddFeatures(JsonElement feature, List<Feature> target, Action<double, double> track)
    {
        int i = 0;
        foreach (var poly in OuterRings(feature, "coordinates", multi: true))
            target.Add(new Feature(i++, Ring(poly, track: true, track)));
    }

    // Yields each polygon's outer ring element. For a MultiPolygon, coordinates is
    // [polygon...] where polygon is [ring...]; for a Polygon it is [ring...].
    private static IEnumerable<JsonElement> OuterRings(JsonElement owner, string prop, bool multi)
    {
        if (!owner.TryGetProperty(prop, out var coords) || coords.ValueKind != JsonValueKind.Array)
            yield break;

        if (multi)
        {
            foreach (var polygon in coords.EnumerateArray())
            {
                if (polygon.ValueKind == JsonValueKind.Array && polygon.GetArrayLength() > 0)
                    yield return polygon[0];
            }
        }
        else if (coords.GetArrayLength() > 0)
        {
            yield return coords[0];
        }
    }

    // Builds an SVG points string from a ring (array of [x, y] pairs).
    private static string Ring(JsonElement ring, bool track, Action<double, double> tracker)
        => Points(ring, track, tracker);

    // Builds an SVG points string from a LineString geometry's coordinates.
    private static string LineString(JsonElement geom, bool track, Action<double, double> tracker)
        => geom.TryGetProperty("coordinates", out var coords) ? Points(coords, track, tracker) : "";

    private static string Points(JsonElement pairs, bool track, Action<double, double> tracker)
    {
        if (pairs.ValueKind != JsonValueKind.Array) return "";
        var sb = new StringBuilder();
        foreach (var p in pairs.EnumerateArray())
        {
            if (p.ValueKind != JsonValueKind.Array || p.GetArrayLength() < 2) continue;
            double x = p[0].GetDouble();
            double y = p[1].GetDouble();
            if (track) tracker(x, y);
            if (sb.Length > 0) sb.Append(' ');
            sb.Append(x.ToString("0.##", CultureInfo.InvariantCulture));
            sb.Append(',');
            sb.Append(y.ToString("0.##", CultureInfo.InvariantCulture));
        }
        return sb.ToString();
    }

    // Yields (geometry, name?) for each member of a GeometryCollection feature.
    private static IEnumerable<(JsonElement Geom, string? Name)> GeometryCollection(JsonElement feature)
    {
        if (!feature.TryGetProperty("geometries", out var geoms) || geoms.ValueKind != JsonValueKind.Array)
            yield break;
        foreach (var g in geoms.EnumerateArray())
        {
            var name = g.TryGetProperty("name", out var n) ? n.GetString() : null;
            yield return (g, name);
        }
    }

    private static double Width(JsonElement el, double fallback)
        => el.TryGetProperty("width", out var w) && w.ValueKind == JsonValueKind.Number
            ? w.GetDouble() : fallback;

    // Area-weighted polygon centroid of a ring (array of [x, y] pairs); used to place
    // a district's name label. Falls back to the vertex average for degenerate rings.
    private static (double Cx, double Cy) Centroid(JsonElement ring)
    {
        if (ring.ValueKind != JsonValueKind.Array) return (0, 0);

        var pts = new List<(double X, double Y)>();
        foreach (var p in ring.EnumerateArray())
        {
            if (p.ValueKind == JsonValueKind.Array && p.GetArrayLength() >= 2)
                pts.Add((p[0].GetDouble(), p[1].GetDouble()));
        }
        if (pts.Count == 0) return (0, 0);

        double area = 0, cx = 0, cy = 0, sumX = 0, sumY = 0;
        for (int i = 0; i < pts.Count; i++)
        {
            var (x0, y0) = pts[i];
            var (x1, y1) = pts[(i + 1) % pts.Count];
            double cross = x0 * y1 - x1 * y0;
            area += cross;
            cx += (x0 + x1) * cross;
            cy += (y0 + y1) * cross;
            sumX += x0;
            sumY += y0;
        }
        area *= 0.5;

        if (Math.Abs(area) < 1e-6)
            return (sumX / pts.Count, sumY / pts.Count);

        return (cx / (6 * area), cy / (6 * area));
    }
}
