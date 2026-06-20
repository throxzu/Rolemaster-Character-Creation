using System.Globalization;
using System.Text.Json;

namespace RolemasterCharacterCreation.Services;

// Parses a Watabou "One-Page Dungeon" export into a flat, render-ready model for inline
// SVG. The dungeon is an axis-aligned grid: "rects" are floor rectangles, doors sit on
// walls, and columns/water/notes decorate cells. Stateless and thread-safe (singleton).
public class DungeonMapService
{
    // Pixels per grid cell — coordinates are scaled by this so stroke widths read well.
    public const double S = 10.0;

    // A floor rectangle. Coordinates are already scaled by S. RoomId groups adjacent
    // non-corridor rects so a single click can reveal/mark a whole room. Rotunda rects
    // are circular rooms (drawn as a circle of radius min(W,H)/2 about the centre).
    public sealed record Rect(int Index, int RoomId, double X, double Y, double W, double H, double Cx, double Cy, bool Rotunda);

    // A note as authored in the uploaded file; used to seed the editable note rows.
    public sealed record NoteSeed(string? Ref, string? Text, double X, double Y);
    public sealed record Wall(double X1, double Y1, double X2, double Y2);
    public sealed record Door(double Cx, double Cy, bool Vertical, double Len);
    public sealed record Pt(double X, double Y);
    public sealed record WaterCell(double X, double Y, double Size);
    public sealed record Note(double Cx, double Cy, string? Ref, string? Text);

    // A single grid square of floor. Cx/Cy are the scaled centre (where a marker sits).
    public sealed record GridCell(int X, int Y, double Cx, double Cy);

    public sealed class DungeonRender
    {
        public double MinX { get; set; }
        public double MinY { get; set; }
        public double Width { get; set; } = 100;
        public double Height { get; set; } = 100;

        public string ViewBox => string.Create(CultureInfo.InvariantCulture,
            $"{MinX:0.##} {MinY:0.##} {Width:0.##} {Height:0.##}");

        public List<Rect> Rects { get; } = [];
        public List<GridCell> Cells { get; } = [];
        public List<Wall> Walls { get; } = [];
        public List<Door> Doors { get; } = [];
        public List<Pt> Columns { get; } = [];
        public List<WaterCell> Water { get; } = [];
        public List<Note> Notes { get; } = [];
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
                !root.TryGetProperty("rects", out var rects) ||
                rects.ValueKind != JsonValueKind.Array ||
                rects.GetArrayLength() == 0)
            {
                error = "Not a dungeon map: expected a Watabou One-Page Dungeon export with a \"rects\" array.";
                return false;
            }
            var first = rects[0];
            if (!first.TryGetProperty("w", out _) || !first.TryGetProperty("h", out _))
            {
                error = "Dungeon \"rects\" are missing width/height.";
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

    public DungeonRender Build(string rawJson)
    {
        var render = new DungeonRender();
        using var doc = JsonDocument.Parse(rawJson);
        var root = doc.RootElement;

        if (!root.TryGetProperty("rects", out var rectsEl) || rectsEl.ValueKind != JsonValueKind.Array)
            return render;

        // ── Read raw integer rects ─────────────────────────────────────────────
        var raw = new List<(int X, int Y, int W, int H, bool Rot)>();
        foreach (var r in rectsEl.EnumerateArray())
        {
            int x = GetInt(r, "x"), y = GetInt(r, "y"), w = GetInt(r, "w"), h = GetInt(r, "h");
            if (w <= 0 || h <= 0) continue;
            bool rot = r.TryGetProperty("rotunda", out var rt) && rt.ValueKind == JsonValueKind.True;
            raw.Add((x, y, w, h, rot));
        }
        if (raw.Count == 0) return render;

        // ── Group rects into rooms (union-find). A "corridor" is a rect only one cell
        //    wide; it stays its own group so it doesn't merge two rooms into one. ──
        int n = raw.Count;
        var parent = new int[n];
        for (int i = 0; i < n; i++) parent[i] = i;
        int Find(int a) { while (parent[a] != a) { parent[a] = parent[parent[a]]; a = parent[a]; } return a; }
        void Union(int a, int b) { parent[Find(a)] = Find(b); }

        bool Corridor((int X, int Y, int W, int H, bool Rot) r) => Math.Min(r.W, r.H) <= 1;

        for (int i = 0; i < n; i++)
            for (int j = i + 1; j < n; j++)
            {
                if (Corridor(raw[i]) || Corridor(raw[j])) continue;
                if (TouchOrOverlap(raw[i], raw[j])) Union(i, j);
            }

        // ── Floor cell set, used to derive the wall outline. Rotunda cells are kept
        //    separate: their outline is drawn as a circle, not square wall segments. ──
        var floor = new HashSet<(int, int)>();
        var rotundaCells = new HashSet<(int, int)>();
        foreach (var r in raw)
            for (int cx = r.X; cx < r.X + r.W; cx++)
                for (int cy = r.Y; cy < r.Y + r.H; cy++)
                {
                    floor.Add((cx, cy));
                    if (r.Rot) rotundaCells.Add((cx, cy));
                }

        // Each distinct floor square is a clickable target for placing markers.
        foreach (var (cx, cy) in floor)
            render.Cells.Add(new GridCell(cx, cy, (cx + 0.5) * S, (cy + 0.5) * S));

        // ── Bounding box (grid units) ──────────────────────────────────────────
        int minX = int.MaxValue, minY = int.MaxValue, maxX = int.MinValue, maxY = int.MinValue;
        foreach (var r in raw)
        {
            minX = Math.Min(minX, r.X); minY = Math.Min(minY, r.Y);
            maxX = Math.Max(maxX, r.X + r.W); maxY = Math.Max(maxY, r.Y + r.H);
        }
        double pad = 1.0;
        render.MinX = (minX - pad) * S;
        render.MinY = (minY - pad) * S;
        render.Width = (maxX - minX + pad * 2) * S;
        render.Height = (maxY - minY + pad * 2) * S;

        // ── Emit scaled rects with a normalized room id ────────────────────────
        for (int i = 0; i < n; i++)
        {
            var r = raw[i];
            double x = r.X * S, y = r.Y * S, w = r.W * S, h = r.H * S;
            render.Rects.Add(new Rect(i, Find(i), x, y, w, h, x + w / 2, y + h / 2, r.Rot));
        }

        // ── Walls: every floor-cell edge that borders a non-floor cell. Rotunda cells
        //    are skipped — their circular outline is rendered separately. ─────────────
        foreach (var (cx, cy) in floor)
        {
            if (rotundaCells.Contains((cx, cy))) continue;
            if (!floor.Contains((cx, cy - 1))) render.Walls.Add(Seg(cx, cy, cx + 1, cy));         // top
            if (!floor.Contains((cx, cy + 1))) render.Walls.Add(Seg(cx, cy + 1, cx + 1, cy + 1)); // bottom
            if (!floor.Contains((cx - 1, cy))) render.Walls.Add(Seg(cx, cy, cx, cy + 1));         // left
            if (!floor.Contains((cx + 1, cy))) render.Walls.Add(Seg(cx + 1, cy, cx + 1, cy + 1)); // right
        }

        // ── Doors ──────────────────────────────────────────────────────────────
        if (root.TryGetProperty("doors", out var doors) && doors.ValueKind == JsonValueKind.Array)
        {
            foreach (var d in doors.EnumerateArray())
            {
                double dx = GetInt(d, "x"), dy = GetInt(d, "y");
                double dirx = 0, diry = 0;
                if (d.TryGetProperty("dir", out var dir) && dir.ValueKind == JsonValueKind.Object)
                {
                    dirx = GetDouble(dir, "x");
                    diry = GetDouble(dir, "y");
                }
                // Centre the door on the wall between the cell and its neighbour.
                double cx = (dx + 0.5 + dirx * 0.5) * S;
                double cy = (dy + 0.5 + diry * 0.5) * S;
                bool vertical = Math.Abs(dirx) > Math.Abs(diry); // wall is vertical → bar runs vertically
                render.Doors.Add(new Door(cx, cy, vertical, S * 0.8));
            }
        }

        // ── Columns / water / notes ────────────────────────────────────────────
        // Columns are point positions in grid units (rotunda pillars use fractional
        // coordinates), drawn directly as small dots.
        foreach (var (x, y) in Cells(root, "columns"))
            render.Columns.Add(new Pt(x * S, y * S));

        foreach (var (x, y) in Cells(root, "water"))
            render.Water.Add(new WaterCell(x * S, y * S, S));

        if (root.TryGetProperty("notes", out var notes) && notes.ValueKind == JsonValueKind.Array)
        {
            foreach (var note in notes.EnumerateArray())
            {
                double nx, ny;
                if (note.TryGetProperty("pos", out var pos) && pos.ValueKind == JsonValueKind.Object)
                { nx = GetDouble(pos, "x"); ny = GetDouble(pos, "y"); }
                else { nx = GetDouble(note, "x"); ny = GetDouble(note, "y"); }

                var text = note.TryGetProperty("text", out var t) ? t.GetString() : null;
                var refr = note.TryGetProperty("ref", out var rf) ? rf.GetString() : null;
                render.Notes.Add(new Note(nx * S, ny * S, refr, text));
            }
        }

        return render;
    }

    private static Wall Seg(int x1, int y1, int x2, int y2) =>
        new(x1 * S, y1 * S, x2 * S, y2 * S);

    // True when two rects overlap or share an edge (touching counts, so adjacent room
    // pieces merge into one room).
    private static bool TouchOrOverlap((int X, int Y, int W, int H, bool Rot) a, (int X, int Y, int W, int H, bool Rot) b) =>
        a.X <= b.X + b.W && b.X <= a.X + a.W &&
        a.Y <= b.Y + b.H && b.Y <= a.Y + a.H;

    // Reads the authored notes (number, text, grid position) for seeding editable rows.
    // Returns grid-unit coordinates (not scaled), matching how notes are stored.
    public List<NoteSeed> ExtractNotes(string rawJson)
    {
        var list = new List<NoteSeed>();
        try
        {
            using var doc = JsonDocument.Parse(rawJson);
            if (!doc.RootElement.TryGetProperty("notes", out var notes) || notes.ValueKind != JsonValueKind.Array)
                return list;

            foreach (var note in notes.EnumerateArray())
            {
                double nx, ny;
                if (note.TryGetProperty("pos", out var pos) && pos.ValueKind == JsonValueKind.Object)
                { nx = GetDouble(pos, "x"); ny = GetDouble(pos, "y"); }
                else { nx = GetDouble(note, "x"); ny = GetDouble(note, "y"); }

                var text = note.TryGetProperty("text", out var t) ? t.GetString() : null;
                var refr = note.TryGetProperty("ref", out var rf) ? rf.GetString() : null;
                list.Add(new NoteSeed(refr, text, nx, ny));
            }
        }
        catch (JsonException) { /* a malformed file simply yields no notes */ }
        return list;
    }

    private static IEnumerable<(double X, double Y)> Cells(JsonElement root, string prop)
    {
        if (!root.TryGetProperty(prop, out var arr) || arr.ValueKind != JsonValueKind.Array)
            yield break;
        foreach (var c in arr.EnumerateArray())
        {
            if (c.ValueKind == JsonValueKind.Object && c.TryGetProperty("x", out _))
                yield return (GetDouble(c, "x"), GetDouble(c, "y"));
        }
    }

    // Reads a numeric field as an int, tolerating values written as decimals (e.g. 2.0)
    // so a stray non-integer never throws a FormatException mid-parse.
    private static int GetInt(JsonElement el, string prop) =>
        el.TryGetProperty(prop, out var v) && v.ValueKind == JsonValueKind.Number
            ? (int)Math.Round(v.GetDouble()) : 0;

    private static double GetDouble(JsonElement el, string prop) =>
        el.TryGetProperty(prop, out var v) && v.ValueKind == JsonValueKind.Number ? v.GetDouble() : 0;
}
