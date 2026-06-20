using System.Globalization;
using System.Text;
using System.Text.Json;

namespace RolemasterCharacterCreation.Services;

// Parses a Watabou "Perilous Shores" hex-region export into a render-ready model for
// inline SVG. Aims for a hand-drawn parchment look: land is clipped to smooth,
// curve-fitted coastlines (the hex grid never shows), with shaded mountain glyphs and
// forest/rock stippling. Pointy-top, even-r layout. Stateless / singleton.
public class WorldMapService
{
    private const double S = 10.0;
    private static readonly double Sqrt3 = Math.Sqrt(3);

    private static readonly (int dq, int dr)[] EvenDirs =
        [(+1, 0), (+1, -1), (0, -1), (-1, 0), (0, +1), (+1, +1)];
    private static readonly (int dq, int dr)[] OddDirs =
        [(+1, 0), (0, -1), (-1, -1), (-1, 0), (-1, +1), (0, +1)];
    private static readonly int[] EdgeForDir = [0, 5, 4, 3, 2, 1];

    public sealed record Hex(int Q, int R, string Points, string Fill);
    public sealed record Line(string Points);
    public sealed record Pt(double X, double Y);
    public sealed record Mtn(string Tri, string Shadow);
    public sealed record ImportedPoi(int Q, int R, string Name, string Category, string? Info);

    public sealed class WorldRender
    {
        public double MinX { get; set; }
        public double MinY { get; set; }
        public double Width { get; set; } = 100;
        public double Height { get; set; } = 100;
        public string ViewBox => string.Create(CultureInfo.InvariantCulture,
            $"{MinX:0.##} {MinY:0.##} {Width:0.##} {Height:0.##}");
        public string MinXs => Fmt(MinX);
        public string MinYs => Fmt(MinY);
        public string Ws => Fmt(Width);
        public string Hs => Fmt(Height);

        public List<(int Q, int R)> Cells { get; } = []; // every hex (land + water), for fog
        public List<Hex> Hexes { get; } = [];        // land hexes only (clipped to Coast)
        public List<Line> Coast { get; } = [];        // smoothed, closed coastline loops
        public List<Mtn> Mountains { get; } = [];
        public List<Pt> Forests { get; } = [];
        public List<Pt> Rocks { get; } = [];
        public List<Line> Roads { get; } = [];
        public List<Line> Rivers { get; } = [];
        public List<Line> SeaRoutes { get; } = [];
    }

    public (double X, double Y) HexCenter(int q, int r)
    {
        double x = S * Sqrt3 * (q + ((r & 1) == 0 ? 0.5 : 0.0));
        double y = S * 1.5 * r;
        return (x, y);
    }

    public bool IsValid(string rawJson, out string? error)
    {
        error = null;
        try
        {
            using var doc = JsonDocument.Parse(rawJson);
            if (doc.RootElement.ValueKind != JsonValueKind.Object ||
                !doc.RootElement.TryGetProperty("hexes", out var hexes) ||
                hexes.ValueKind != JsonValueKind.Object ||
                !hexes.EnumerateObject().Any())
            {
                error = "Not a world map: expected a Perilous Shores hex export with \"hexes\".";
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

    public WorldRender Build(string rawJson)
    {
        var render = new WorldRender();
        double minX = double.MaxValue, minY = double.MaxValue, maxX = double.MinValue, maxY = double.MinValue;
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

        var land = new HashSet<(int, int)>();
        var cells = new List<(int q, int r, string? terrain)>();

        if (root.TryGetProperty("hexes", out var hexes) && hexes.ValueKind == JsonValueKind.Object)
        {
            foreach (var prop in hexes.EnumerateObject())
            {
                var h = prop.Value;
                if (!h.TryGetProperty("q", out var qe) || !h.TryGetProperty("r", out var re)) continue;
                int q = qe.GetInt32(), r = re.GetInt32();
                var terrain = h.TryGetProperty("terrain", out var t) ? t.GetString() : null;
                cells.Add((q, r, terrain));
                if (terrain != "water") land.Add((q, r));
            }
        }

        foreach (var (q, r, terrain) in cells)
        {
            render.Cells.Add((q, r));
            var (cx, cy) = HexCenter(q, r);
            // Track bbox over every cell so the sea fills the whole map extent.
            for (int i = 0; i < 6; i++)
            {
                var (x, y) = Corner(cx, cy, i);
                Track(x, y);
            }

            if (terrain == "water") continue; // sea is drawn as a flat rect behind the clip

            render.Hexes.Add(new Hex(q, r, HexPolygon(cx, cy), TerrainFill(terrain)));

            switch (terrain)
            {
                case "mountain":
                    render.Mountains.Add(MountainGlyph(cx, cy));
                    break;
                case "forest-dark":
                case "forest-light":
                    render.Forests.Add(Jitter(cx, cy, q, r, 0));
                    render.Forests.Add(Jitter(cx, cy, q, r, 1));
                    break;
                case "rocks":
                    render.Rocks.Add(new Pt(cx, cy));
                    break;
            }
        }

        // Smooth coastlines: chain land/water boundary edges into closed loops, then
        // round them off with Chaikin corner-cutting so islands look hand-drawn.
        foreach (var loop in BoundaryLoops(land))
        {
            var smooth = Chaikin(loop, 2);
            var sb = new StringBuilder();
            foreach (var (x, y) in smooth) AppendPoint(sb, x, y);
            if (sb.Length > 0) render.Coast.Add(new Line(sb.ToString()));
        }

        foreach (var line in KeyPathLines(root, "roads")) render.Roads.Add(line);
        foreach (var line in KeyPathLines(root, "searoutes")) render.SeaRoutes.Add(line);

        if (root.TryGetProperty("rivers", out var rivers) && rivers.ValueKind == JsonValueKind.Object)
        {
            foreach (var prop in rivers.EnumerateObject())
            {
                if (!prop.Value.TryGetProperty("channel", out var channel) ||
                    channel.ValueKind != JsonValueKind.Array) continue;

                var pts = new List<(double, double)>();
                foreach (var edge in channel.EnumerateArray())
                {
                    var parts = edge.GetString()?.Split(':');
                    if (parts is not { Length: 2 }) continue;
                    if (ParseKey(parts[0]) is not { } a || ParseKey(parts[1]) is not { } b) continue;
                    var (ax, ay) = HexCenter(a.Q, a.R);
                    var (bx, by) = HexCenter(b.Q, b.R);
                    pts.Add(((ax + bx) / 2, (ay + by) / 2));
                }
                if (pts.Count < 2) continue;
                var smooth = ChaikinOpen(pts, 2);
                var sb = new StringBuilder();
                foreach (var (x, y) in smooth) AppendPoint(sb, x, y);
                render.Rivers.Add(new Line(sb.ToString()));
            }
        }

        if (any)
        {
            double pad = S;
            render.MinX = minX - pad;
            render.MinY = minY - pad;
            render.Width = (maxX - minX) + pad * 2;
            render.Height = (maxY - minY) + pad * 2;
        }

        return render;
    }

    public List<ImportedPoi> ExtractPois(string rawJson)
    {
        var pois = new List<ImportedPoi>();
        using var doc = JsonDocument.Parse(rawJson);
        if (!doc.RootElement.TryGetProperty("hexes", out var hexes) || hexes.ValueKind != JsonValueKind.Object)
            return pois;

        foreach (var prop in hexes.EnumerateObject())
        {
            var h = prop.Value;
            if (!h.TryGetProperty("q", out var qe) || !h.TryGetProperty("r", out var re)) continue;
            int q = qe.GetInt32(), r = re.GetInt32();

            if (h.TryGetProperty("town", out var town) && town.ValueKind == JsonValueKind.Object)
            {
                var name = town.TryGetProperty("name", out var n) ? n.GetString() : null;
                var type = town.TryGetProperty("type", out var ty) ? ty.GetString() : null;
                var info = town.TryGetProperty("info", out var i) ? i.GetString() : null;
                pois.Add(new ImportedPoi(q, r, name ?? "Settlement", CategoryForTownType(type), info));
            }
            else if (h.TryGetProperty("danger", out var danger) && danger.ValueKind == JsonValueKind.Object)
            {
                var name = danger.TryGetProperty("name", out var n) ? n.GetString() : null;
                pois.Add(new ImportedPoi(q, r, name ?? "Site", "Dungeon", null));
            }
        }
        return pois;
    }

    private static string CategoryForTownType(string? type) => type?.ToLowerInvariant() switch
    {
        "city" => "City",
        "village" => "Village",
        _ => "Town",
    };

    // ── Coastline construction ──────────────────────────────────────────────

    private List<List<(double X, double Y)>> BoundaryLoops(HashSet<(int, int)> land)
    {
        var coords = new Dictionary<long, (double X, double Y)>();
        var adj = new Dictionary<long, List<long>>();

        long Key(double x, double y)
        {
            long xi = (long)Math.Round(x * 4);
            long yi = (long)Math.Round(y * 4);
            return (xi << 32) ^ (yi & 0xffffffff);
        }
        void AddEdge(double ax, double ay, double bx, double by)
        {
            long ka = Key(ax, ay), kb = Key(bx, by);
            coords[ka] = (ax, ay);
            coords[kb] = (bx, by);
            (adj.TryGetValue(ka, out var la) ? la : adj[ka] = []).Add(kb);
            (adj.TryGetValue(kb, out var lb) ? lb : adj[kb] = []).Add(ka);
        }

        foreach (var (q, r) in land)
        {
            var (cx, cy) = HexCenter(q, r);
            var dirs = (r & 1) == 0 ? EvenDirs : OddDirs;
            for (int d = 0; d < 6; d++)
            {
                if (land.Contains((q + dirs[d].dq, r + dirs[d].dr))) continue;
                int edge = EdgeForDir[d];
                var (x1, y1) = Corner(cx, cy, edge);
                var (x2, y2) = Corner(cx, cy, (edge + 1) % 6);
                AddEdge(x1, y1, x2, y2);
            }
        }

        var loops = new List<List<(double, double)>>();
        var used = new HashSet<(long, long)>();
        static (long, long) Norm(long a, long b) => a < b ? (a, b) : (b, a);

        foreach (var startKey in adj.Keys)
        {
            foreach (var firstNext in adj[startKey])
            {
                if (used.Contains(Norm(startKey, firstNext))) continue;

                var loop = new List<(double, double)> { coords[startKey] };
                long from = startKey, to = firstNext;
                while (true)
                {
                    used.Add(Norm(from, to));
                    loop.Add(coords[to]);
                    if (to == startKey) break;

                    long next = -1;
                    foreach (var cand in adj[to])
                    {
                        if (cand == from) continue;
                        if (used.Contains(Norm(to, cand))) continue;
                        next = cand;
                        break;
                    }
                    if (next == -1) break;
                    from = to;
                    to = next;
                }
                if (loop.Count >= 4) loops.Add(loop);
            }
        }
        return loops;
    }

    // Chaikin corner-cutting for a closed ring.
    private static List<(double X, double Y)> Chaikin(List<(double X, double Y)> p, int iters)
    {
        for (int it = 0; it < iters; it++)
        {
            var np = new List<(double, double)>(p.Count * 2);
            int n = p.Count;
            for (int i = 0; i < n; i++)
            {
                var a = p[i];
                var b = p[(i + 1) % n];
                np.Add((0.75 * a.X + 0.25 * b.X, 0.75 * a.Y + 0.25 * b.Y));
                np.Add((0.25 * a.X + 0.75 * b.X, 0.25 * a.Y + 0.75 * b.Y));
            }
            p = np;
        }
        return p;
    }

    // Chaikin for an open polyline (keeps the endpoints).
    private static List<(double X, double Y)> ChaikinOpen(List<(double X, double Y)> p, int iters)
    {
        for (int it = 0; it < iters; it++)
        {
            var np = new List<(double, double)> { p[0] };
            for (int i = 0; i < p.Count - 1; i++)
            {
                var a = p[i];
                var b = p[i + 1];
                np.Add((0.75 * a.X + 0.25 * b.X, 0.75 * a.Y + 0.25 * b.Y));
                np.Add((0.25 * a.X + 0.75 * b.X, 0.25 * a.Y + 0.75 * b.Y));
            }
            np.Add(p[^1]);
            p = np;
        }
        return p;
    }

    // ── Geometry helpers ────────────────────────────────────────────────────

    private IEnumerable<Line> KeyPathLines(JsonElement root, string prop)
    {
        if (!root.TryGetProperty(prop, out var coll) || coll.ValueKind != JsonValueKind.Object)
            yield break;

        foreach (var path in coll.EnumerateObject())
        {
            if (path.Value.ValueKind != JsonValueKind.Array) continue;
            var pts = new List<(double, double)>();
            foreach (var keyEl in path.Value.EnumerateArray())
            {
                if (ParseKey(keyEl.GetString()) is not { } k) continue;
                pts.Add(HexCenter(k.Q, k.R));
            }
            if (pts.Count < 2) continue;
            var smooth = ChaikinOpen(pts, 2);
            var sb = new StringBuilder();
            foreach (var (x, y) in smooth) AppendPoint(sb, x, y);
            yield return new Line(sb.ToString());
        }
    }

    private static (double X, double Y) Corner(double cx, double cy, int i)
    {
        double ang = Math.PI / 180.0 * (60 * i - 30);
        return (cx + S * Math.Cos(ang), cy + S * Math.Sin(ang));
    }

    private string HexPolygon(double cx, double cy)
    {
        var sb = new StringBuilder();
        for (int i = 0; i < 6; i++)
        {
            var (x, y) = Corner(cx, cy, i);
            AppendPoint(sb, x, y);
        }
        return sb.ToString();
    }

    // Full hex outline for any (q, r) — used for fog masks and the fog click layer.
    public string HexPolygonPoints(int q, int r)
    {
        var (cx, cy) = HexCenter(q, r);
        return HexPolygon(cx, cy);
    }

    private static Mtn MountainGlyph(double cx, double cy)
    {
        double px = cx, py = cy - 0.6 * S;            // peak
        double lx = cx - 0.55 * S, by = cy + 0.4 * S; // base left
        double rx = cx + 0.55 * S;                    // base right
        var tri = string.Create(CultureInfo.InvariantCulture,
            $"{px:0.##},{py:0.##} {lx:0.##},{by:0.##} {rx:0.##},{by:0.##}");
        var shadow = string.Create(CultureInfo.InvariantCulture,
            $"{px:0.##},{py:0.##} {cx:0.##},{by:0.##} {rx:0.##},{by:0.##}");
        return new Mtn(tri, shadow);
    }

    private static Pt Jitter(double cx, double cy, int q, int r, int k)
    {
        int h = HashCode.Combine(q, r, k);
        double ox = ((h & 0xff) / 255.0 - 0.5) * 0.75 * S;
        double oy = (((h >> 8) & 0xff) / 255.0 - 0.5) * 0.75 * S;
        return new Pt(cx + ox, cy + oy);
    }

    private static void AppendPoint(StringBuilder sb, double x, double y)
    {
        if (sb.Length > 0) sb.Append(' ');
        sb.Append(x.ToString("0.##", CultureInfo.InvariantCulture));
        sb.Append(',');
        sb.Append(y.ToString("0.##", CultureInfo.InvariantCulture));
    }

    private static string Fmt(double v) => v.ToString("0.##", CultureInfo.InvariantCulture);

    private static (int Q, int R)? ParseKey(string? key)
    {
        if (string.IsNullOrEmpty(key) || key[0] != 'q') return null;
        int sep = key.IndexOf("_r", StringComparison.Ordinal);
        if (sep < 1) return null;
        if (int.TryParse(key.AsSpan(1, sep - 1), out var q) &&
            int.TryParse(key.AsSpan(sep + 2), out var r))
            return (q, r);
        return null;
    }

    private static string TerrainFill(string? terrain) => terrain switch
    {
        "swamp" => "#d4d4ba",
        "forest-light" => "#e3e6d0",
        "forest-dark" => "#d7ddc0",
        "mountain" => "#ece9e2",
        "rocks" => "#e2ddd1",
        _ => "#f1ede1", // plains / grassland
    };
}
