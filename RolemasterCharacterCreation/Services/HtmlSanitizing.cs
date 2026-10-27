using System.Text.RegularExpressions;
using Ganss.Xss;

namespace RolemasterCharacterCreation.Services;

/// <summary>
/// Allowlist sanitising for the two places where stored markup reaches a browser as markup
/// rather than as text: session notes, and the uploaded building/cave maps.
///
/// Both sanitisers run <em>on save and again on render</em>. The render-side pass is not
/// belt-and-braces — it is what protects rows that were written before this existed, and
/// anything a future code path stores without going through a save handler.
/// </summary>
public static class HtmlSanitizing
{
    /// <summary>Where a session-note image may be loaded from. Nothing off-site.</summary>
    private const string ImagePathPrefix = "/session-notes/image/";

    // Quill tags each formatted block with its own classes. Only the ones it actually emits
    // are allowed through, so a note cannot borrow a layout class from the rest of the app.
    private static readonly string[] QuillClasses =
    [
        "ql-align-center", "ql-align-right", "ql-align-justify",
        "ql-indent-1", "ql-indent-2", "ql-indent-3", "ql-indent-4",
        "ql-indent-5", "ql-indent-6", "ql-indent-7", "ql-indent-8",
        "ql-syntax",
    ];

    private static readonly HtmlSanitizer NotesSanitizer = BuildNotesSanitizer();
    private static readonly HtmlSanitizer SvgSanitizer   = BuildSvgSanitizer();

    // One sanitiser instance is shared across requests. HtmlSanitizer is documented as
    // thread-safe for sanitising once configured, and configuration happens here only.
    private static readonly object NotesLock = new();
    private static readonly object SvgLock   = new();

    /// <summary>
    /// Elements whose <em>contents</em> must go with them. The sanitiser below unwraps a tag it
    /// does not recognise and keeps the words inside — which is what you want for a stray
    /// &lt;font&gt; from a Word paste, but means a &lt;script&gt; block would leave its source
    /// showing as text in the middle of the recap.
    /// </summary>
    private static readonly Regex CodeBlocks = new(
        @"<\s*(script|style|noscript|template|iframe|object|embed|applet)\b[^>]*>.*?<\s*/\s*\1\s*>"
        + @"|<\s*(script|style|noscript|template|iframe|object|embed|applet)\b[^>]*/?>",
        RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);

    /// <summary>Rich text written by the GM, shown to the GM and — for public notes — to players.</summary>
    public static string Notes(string? html)
    {
        if (string.IsNullOrWhiteSpace(html)) return string.Empty;

        // Tidying, not the security boundary: the allowlist below is what makes this safe, and
        // it runs on the result whatever this pass does or does not manage to match.
        var tidied = CodeBlocks.Replace(html, string.Empty);

        lock (NotesLock) return NotesSanitizer.Sanitize(tidied);
    }

    /// <summary>
    /// An uploaded Watabou map. These are drawings, so only geometry and presentation survive;
    /// script, event handlers and anything that could reach off-site are removed.
    /// </summary>
    public static string MapSvg(string? svg)
    {
        if (string.IsNullOrWhiteSpace(svg)) return string.Empty;
        lock (SvgLock) return SvgSanitizer.Sanitize(svg);
    }

    private static HtmlSanitizer BuildNotesSanitizer()
    {
        var s = new HtmlSanitizer();

        s.AllowedTags.Clear();
        foreach (var t in new[]
        {
            "p", "br", "span", "div",
            "strong", "b", "em", "i", "u", "s", "sub", "sup",
            "h1", "h2", "h3", "h4",
            "ul", "ol", "li", "blockquote", "pre", "code", "hr",
            "a", "img",
            "table", "thead", "tbody", "tr", "th", "td",
        }) s.AllowedTags.Add(t);

        s.AllowedAttributes.Clear();
        foreach (var a in new[] { "class", "href", "src", "alt", "title", "width", "height",
                                  "colspan", "rowspan", "target", "rel",
                                  // Quill 2 writes every list as <ol> and says which kind it is
                                  // here. Strip it and a bullet list silently becomes numbered.
                                  "data-list" })
            s.AllowedAttributes.Add(a);

        // No inline styles at all: `style` is the usual way a sanitised note still manages to
        // cover the page or smuggle a url(). Quill's formatting rides on classes instead.
        s.AllowedCssProperties.Clear();
        s.AllowedAtRules.Clear();

        s.AllowedSchemes.Clear();
        foreach (var scheme in new[] { "http", "https", "mailto" }) s.AllowedSchemes.Add(scheme);

        s.AllowDataAttributes = false;
        s.KeepChildNodes = true;   // unwrap a disallowed tag rather than deleting the text inside it

        s.PostProcessNode += (_, e) =>
        {
            if (e.Node is not AngleSharp.Dom.IElement el) return;

            switch (el.TagName.ToLowerInvariant())
            {
                case "a":
                    // A note is read by players; a link out of it opens detached from this tab.
                    el.SetAttribute("target", "_blank");
                    el.SetAttribute("rel", "noopener noreferrer");
                    break;

                case "img":
                    // Only our own image endpoint. An off-site src would let a note report back
                    // who read it and when, and is the usual shape of a tracking pixel.
                    var src = el.GetAttribute("src") ?? "";
                    if (!src.StartsWith(ImagePathPrefix, StringComparison.Ordinal))
                        el.Remove();
                    else
                        el.SetAttribute("loading", "lazy");
                    break;
            }

            // Quill puts an empty <span class="ql-ui"> inside every list item to hang its marker
            // on. The class is not on the list above, so what would survive is an empty span.
            if (string.Equals(el.TagName, "span", StringComparison.OrdinalIgnoreCase)
                && el.ChildElementCount == 0
                && string.IsNullOrWhiteSpace(el.TextContent))
            {
                el.Remove();
                return;
            }

            // Strip any class we did not put there ourselves.
            var cls = el.GetAttribute("class");
            if (cls is not null)
            {
                var kept = cls.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                              .Where(c => QuillClasses.Contains(c))
                              .ToArray();
                if (kept.Length == 0) el.RemoveAttribute("class");
                else el.SetAttribute("class", string.Join(' ', kept));
            }
        };

        return s;
    }

    private static HtmlSanitizer BuildSvgSanitizer()
    {
        var s = new HtmlSanitizer();

        s.AllowedTags.Clear();
        foreach (var t in new[]
        {
            "svg", "g", "defs", "symbol", "use", "title", "desc",
            "path", "rect", "circle", "ellipse", "line", "polyline", "polygon",
            "text", "tspan", "textpath",
            "clippath", "mask", "pattern", "marker", "image",
            "lineargradient", "radialgradient", "stop",
            // Watabou's exports lean on a <style> block for fills and strokes; dropping it
            // flattens the map. CSS cannot execute script, and the @rule and url() handling
            // below is what keeps it from fetching anything.
            "style",
        }) s.AllowedTags.Add(t);

        s.AllowedAttributes.Clear();
        foreach (var a in new[]
        {
            "id", "class", "style", "transform", "viewbox", "xmlns", "version",
            "width", "height", "x", "y", "x1", "y1", "x2", "y2", "cx", "cy", "r", "rx", "ry",
            "d", "points", "fill", "fill-opacity", "fill-rule", "stroke", "stroke-width",
            "stroke-opacity", "stroke-linecap", "stroke-linejoin", "stroke-dasharray",
            "stroke-dashoffset", "stroke-miterlimit", "opacity", "font-family", "font-size",
            "font-weight", "font-style", "text-anchor", "dominant-baseline", "letter-spacing",
            "dx", "dy", "offset", "stop-color", "stop-opacity", "gradientunits",
            "gradienttransform", "patternunits", "patterntransform", "clip-path", "clip-rule",
            "mask", "marker-end", "marker-start", "marker-mid", "preserveaspectratio",
            "vector-effect", "paint-order", "shape-rendering", "href",
        }) s.AllowedAttributes.Add(a);

        // Watabou's maps carry exactly one thing in <style>: an @import for the display font
        // the map was drawn with. Blocking it outright costs every existing map its lettering,
        // so the rule is kept and the *host* is what gets checked — see FontHosts below. The
        // app's own pages already load from that host (App.razor), so this adds no new trust.
        s.AllowedAtRules.Clear();
        s.AllowedAtRules.Add(AngleSharp.Css.Dom.CssRuleType.Import);
        s.AllowedAtRules.Add(AngleSharp.Css.Dom.CssRuleType.FontFace);

        // `https` is needed for that font import to survive the CSS pass. It does not widen
        // what the markup can reach: every href is forced back to a local #fragment below.
        s.AllowedSchemes.Clear();
        s.AllowedSchemes.Add("https");

        // The default property list is written for HTML and has no fill or stroke, which would
        // quietly strip any map that styles by rule rather than by attribute.
        foreach (var p in new[]
        {
            "fill", "fill-opacity", "fill-rule", "stroke", "stroke-width", "stroke-opacity",
            "stroke-linecap", "stroke-linejoin", "stroke-dasharray", "stroke-dashoffset",
            "stroke-miterlimit", "paint-order", "vector-effect", "shape-rendering",
            "dominant-baseline", "text-anchor", "clip-path", "clip-rule", "mask", "marker-end",
            "marker-start", "marker-mid", "stop-color", "stop-opacity",
        }) s.AllowedCssProperties.Add(p);

        s.AllowDataAttributes = false;

        // Unlike the notes sanitiser, a disallowed element here takes its children with it.
        // Every element a real Watabou export uses is on the list above, so nothing legitimate
        // is lost, and it stops <script>alert(1)</script> from unwrapping into visible text
        // reading "alert(1)" across the middle of the map.
        s.KeepChildNodes = false;

        s.PostProcessNode += (_, e) =>
        {
            if (e.Node is not AngleSharp.Dom.IElement el) return;

            // The rules inside <style> are text, not attributes, so the attribute pass below
            // never sees them. A url() in here fetches from the player's browser, and CSS can
            // leak what it sees through one, so anything but a local reference or the font
            // host takes the whole block with it rather than being repaired in place.
            if (string.Equals(el.TagName, "style", StringComparison.OrdinalIgnoreCase))
            {
                var css = el.TextContent;
                if (css.Contains("javascript:", StringComparison.OrdinalIgnoreCase)
                    || css.Contains("expression(", StringComparison.OrdinalIgnoreCase)
                    || HasDisallowedUrl(css))
                {
                    el.Remove();
                }
                return;
            }

            // <use href> and <image href> may only point inside this same document.
            foreach (var attr in new[] { "href", "xlink:href" })
            {
                var v = el.GetAttribute(attr);
                if (v is not null && !v.StartsWith('#')) el.RemoveAttribute(attr);
            }

            // url(#localGradient) is how fills reference defs and must survive; url(http…) is
            // how a stylesheet phones home and must not.
            var style = el.GetAttribute("style");
            if (style is not null && HasDisallowedUrl(style)) el.RemoveAttribute("style");
        };

        return s;
    }

    /// <summary>
    /// The only off-site hosts a map may reach, and only for a webfont. App.razor already
    /// loads the site's own typeface from these, so nothing new is being trusted.
    /// </summary>
    private static readonly string[] FontHosts = ["fonts.googleapis.com", "fonts.gstatic.com"];

    /// <summary>
    /// True when a CSS fragment contains a url() pointing anywhere except inside this document
    /// or at the font hosts. `url(#gradient)` is how a fill references a &lt;defs&gt; entry and
    /// must survive; `url(https://anywhere/else)` is how a stylesheet reports back and must not.
    /// </summary>
    private static bool HasDisallowedUrl(string css)
    {
        int i = 0;
        while ((i = css.IndexOf("url(", i, StringComparison.OrdinalIgnoreCase)) >= 0)
        {
            int start = i + 4;
            int end = css.IndexOf(')', start);
            if (end < 0) return true;                       // unterminated: assume the worst
            var target = css[start..end].Trim().Trim('"', '\'');

            if (!target.StartsWith('#')
                && !(Uri.TryCreate(target, UriKind.Absolute, out var u)
                     && u.Scheme == Uri.UriSchemeHttps
                     && FontHosts.Contains(u.Host, StringComparer.OrdinalIgnoreCase)))
                return true;

            i = end + 1;
        }
        return false;
    }
}
