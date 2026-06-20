using System.Globalization;
using System.Text.RegularExpressions;

namespace RolemasterCharacterCreation.Services;

// Prepares an uploaded SVG cave map (e.g. a Watabou "Caves" export) for interactive
// display. Unlike the grid-based dungeon, a cave is a single organic drawing, so we keep
// the SVG exactly as authored and overlay a uniform square grid — that grid drives fog
// of war and legend placement. Stateless / thread-safe (singleton).
public partial class CaveMapService
{
    // Roughly how many grid squares span the map's longer side. The actual cell size is
    // derived per-map from the SVG's dimensions, so grid density stays consistent
    // regardless of the export's coordinate scale.
    private const double TargetCellsAcross = 30.0;

    public sealed class CaveRender
    {
        public double Width { get; set; } = 1000;
        public double Height { get; set; } = 1000;
        public double Cell { get; set; } = 32;
        public int Cols { get; set; }
        public int Rows { get; set; }

        // The cleaned <svg>…</svg> markup to nest beneath the interactive overlay.
        public string Svg { get; set; } = "";

        public string ViewBox => string.Create(CultureInfo.InvariantCulture,
            $"0 0 {Width:0.##} {Height:0.##}");

        public IEnumerable<(int X, int Y)> Cells()
        {
            for (int y = 0; y < Rows; y++)
                for (int x = 0; x < Cols; x++)
                    yield return (x, y);
        }
    }

    // Quick sanity check used by the upload page before persisting an unknown file.
    public bool IsValid(string raw, out string? error)
    {
        error = null;
        if (string.IsNullOrWhiteSpace(raw) ||
            raw.IndexOf("<svg", StringComparison.OrdinalIgnoreCase) < 0 ||
            raw.IndexOf("</svg>", StringComparison.OrdinalIgnoreCase) < 0)
        {
            error = "Not a cave map: expected an SVG file (e.g. a Watabou \"Caves\" export).";
            return false;
        }
        return true;
    }

    public CaveRender Build(string raw)
    {
        var render = new CaveRender { Svg = ExtractSvg(raw) };
        var (w, h) = ReadSize(render.Svg);
        render.Width = w;
        render.Height = h;
        // A per-map cell size keeps the grid ~TargetCellsAcross squares wide whatever the
        // SVG's coordinate scale. Deterministic from the (fixed) markup, so stored cell
        // indices stay valid for the life of the map.
        render.Cell = Math.Max(1, Math.Max(w, h) / TargetCellsAcross);
        render.Cols = Math.Max(1, (int)Math.Ceiling(w / render.Cell));
        render.Rows = Math.Max(1, (int)Math.Ceiling(h / render.Cell));
        return render;
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
}
