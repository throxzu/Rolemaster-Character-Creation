using System.Globalization;
using System.Text.RegularExpressions;

namespace RolemasterCharacterCreation.Services;

// Prepares an uploaded SVG building map (e.g. a Watabou "lonely house" / building export)
// for interactive display. A building SVG is a horizontal strip of square floor cells laid
// side by side (Basement / Ground Floor / 1F…), each labelled by a short <text> in its
// corner. We keep the SVG exactly as drawn and overlay a uniform square grid per floor —
// that grid drives fog of war and legend placement. The displayed floor is chosen by
// cropping the overlay <svg> viewBox to a single floor's square. Stateless (singleton).
public partial class BuildingMapService
{
    // Roughly how many grid squares span one floor's longer side. Buildings are small, so
    // this is lower than the cave's density to keep cells usefully large.
    private const double TargetCellsAcross = 18.0;

    public sealed class Floor
    {
        public int Index { get; set; }
        public string Name { get; set; } = "";

        // Pixel offset of this floor's square within the full strip, and its size.
        public double OriginX { get; set; }
        public double FloorWidth { get; set; }
        public double FloorHeight { get; set; }

        public double Cell { get; set; } = 32;
        public int Cols { get; set; }
        public int Rows { get; set; }

        // Crops the shared embedded drawing down to just this floor's square.
        public string ViewBox => string.Create(CultureInfo.InvariantCulture,
            $"{OriginX:0.##} 0 {FloorWidth:0.##} {FloorHeight:0.##}");

        // Local cell coordinates (0..Cols / 0..Rows). Render at OriginX + CellX*Cell.
        public IEnumerable<(int X, int Y)> Cells()
        {
            for (int y = 0; y < Rows; y++)
                for (int x = 0; x < Cols; x++)
                    yield return (x, y);
        }
    }

    public sealed class BuildingRender
    {
        public double Width { get; set; } = 1000;
        public double Height { get; set; } = 1000;

        // The cleaned <svg>…</svg> markup, shared by every floor (rendered once).
        public string Svg { get; set; } = "";

        public List<Floor> Floors { get; set; } = [];
    }

    // Quick sanity check used by the upload page before persisting an unknown file.
    public bool IsValid(string raw, out string? error)
    {
        error = null;
        if (string.IsNullOrWhiteSpace(raw) ||
            raw.IndexOf("<svg", StringComparison.OrdinalIgnoreCase) < 0 ||
            raw.IndexOf("</svg>", StringComparison.OrdinalIgnoreCase) < 0)
        {
            error = "Not a building map: expected an SVG file (e.g. a Watabou building export).";
            return false;
        }
        return true;
    }

    public BuildingRender Build(string raw)
    {
        var render = new BuildingRender { Svg = ExtractSvg(raw) };
        var (w, h) = ReadSize(render.Svg);
        render.Width = w;
        render.Height = h;

        // Floors are square cells laid out horizontally, so their count is the strip's
        // width/height ratio. Each floor is an equal slice of the full width.
        int floorCount = Math.Max(1, (int)Math.Round(w / Math.Max(1, h)));
        double floorWidth = w / floorCount;

        var names = ReadFloorNames(render.Svg, floorCount);
        double cell = Math.Max(1, Math.Max(floorWidth, h) / TargetCellsAcross);

        for (int i = 0; i < floorCount; i++)
        {
            render.Floors.Add(new Floor
            {
                Index = i,
                Name = names[i],
                OriginX = i * floorWidth,
                FloorWidth = floorWidth,
                FloorHeight = h,
                Cell = cell,
                Cols = Math.Max(1, (int)Math.Ceiling(floorWidth / cell)),
                Rows = Math.Max(1, (int)Math.Ceiling(h / cell)),
            });
        }

        return render;
    }

    // Floor labels are short <text> elements (e.g. B, GF, 1F) drawn in document order, one
    // per floor. Use them when their count matches; otherwise fall back to "Floor N".
    private static List<string> ReadFloorNames(string svg, int floorCount)
    {
        var labels = FloorLabel().Matches(svg)
            .Select(m => m.Groups[1].Value.Trim())
            .Where(s => s.Length is > 0 and <= 4)
            .ToList();

        var names = new List<string>(floorCount);
        for (int i = 0; i < floorCount; i++)
            names.Add(i < labels.Count ? labels[i] : $"Floor {i + 1}");
        return names;
    }

    // Trims to just the <svg>…</svg> element (dropping any XML declaration / doctype) and
    // strips <script> blocks so the embedded markup can never run code.
    private static string ExtractSvg(string raw)
    {
        int start = raw.IndexOf("<svg", StringComparison.OrdinalIgnoreCase);
        int end = raw.LastIndexOf("</svg>", StringComparison.OrdinalIgnoreCase);
        var svg = start >= 0 && end > start ? raw[start..(end + 6)] : raw;
        return ScriptTag().Replace(svg, "");
    }

    // Reads the drawing's pixel size from the root <svg> tag — width/height first, then
    // the viewBox's width/height — falling back to a square default.
    private static (double W, double H) ReadSize(string svg)
    {
        int tagEnd = svg.IndexOf('>');
        var head = tagEnd > 0 ? svg[..tagEnd] : svg;

        double w = ReadDim(head, "width");
        double h = ReadDim(head, "height");

        if (w <= 0 || h <= 0)
        {
            var vb = ViewBoxAttr().Match(head);
            if (vb.Success)
            {
                var parts = vb.Groups[1].Value.Split([' ', ','], StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 4 &&
                    double.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out var vw) &&
                    double.TryParse(parts[3], NumberStyles.Float, CultureInfo.InvariantCulture, out var vh))
                {
                    if (w <= 0) w = vw;
                    if (h <= 0) h = vh;
                }
            }
        }

        if (w <= 0) w = 1000;
        if (h <= 0) h = 1000;
        return (w, h);
    }

    private static double ReadDim(string head, string attr)
    {
        var m = Regex.Match(head, attr + @"\s*=\s*[""']\s*([\d.]+)", RegexOptions.IgnoreCase);
        return m.Success &&
               double.TryParse(m.Groups[1].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var v)
            ? v : 0;
    }

    [GeneratedRegex(@"<script[\s\S]*?</script\s*>", RegexOptions.IgnoreCase)]
    private static partial Regex ScriptTag();

    [GeneratedRegex(@"viewBox\s*=\s*[""']([^""']+)[""']", RegexOptions.IgnoreCase)]
    private static partial Regex ViewBoxAttr();

    [GeneratedRegex(@"<text[^>]*>([^<]{1,4})</text>", RegexOptions.IgnoreCase)]
    private static partial Regex FloorLabel();
}
