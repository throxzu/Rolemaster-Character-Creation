using Microsoft.EntityFrameworkCore;
using RolemasterCharacterCreation.Data;
using RolemasterCharacterCreation.Identity;
using RolemasterCharacterCreation.Models;

namespace RolemasterCharacterCreation.Endpoints;

/// <summary>
/// Upload and delivery for pictures pasted into session notes. These are the only HTTP
/// endpoints in the app — everything else is a Blazor component — because an &lt;img src&gt;
/// needs a URL a browser can fetch on its own.
/// </summary>
public static class SessionNoteImageEndpoints
{
    /// <summary>Where a note's &lt;img src&gt; points. The notes sanitiser allows no other prefix.</summary>
    public const string PathPrefix = "/session-notes/image";

    // The browser re-encodes a paste down to ~1600px before sending, so the display copy is
    // small; the original is whatever came off the camera and is allowed to be much larger.
    private const long MaxDisplayBytes  = 2L * 1024 * 1024;
    private const long MaxOriginalBytes = 12L * 1024 * 1024;

    // Raster formats only. image/svg+xml is deliberately absent: an SVG is a document that can
    // carry script, and serving one from our own origin would hand it the session cookie.
    private static readonly string[] AllowedTypes =
        ["image/png", "image/jpeg", "image/webp", "image/gif"];

    // Magic bytes, so a .png that is really something else is refused whatever it claims.
    private static bool LooksLikeImage(byte[] b) =>
        (b.Length > 8  && b[0] == 0x89 && b[1] == 0x50 && b[2] == 0x4E && b[3] == 0x47) ||  // PNG
        (b.Length > 3  && b[0] == 0xFF && b[1] == 0xD8 && b[2] == 0xFF) ||                  // JPEG
        (b.Length > 12 && b[0] == 0x52 && b[1] == 0x49 && b[2] == 0x46 && b[3] == 0x46
                       && b[8] == 0x57 && b[9] == 0x45 && b[10] == 0x42 && b[11] == 0x50) || // WEBP
        (b.Length > 5  && b[0] == 0x47 && b[1] == 0x49 && b[2] == 0x46);                    // GIF

    public static void MapSessionNoteImages(this WebApplication app)
    {
        // ── Upload ────────────────────────────────────────────────────────────
        // Only the GM writes session notes, so only the GM may add pictures to them.
        app.MapPost(PathPrefix, async (HttpRequest req, AppDbContext db) =>
        {
            if (!req.HasFormContentType) return Results.BadRequest("Expected a multipart form.");

            var form    = await req.ReadFormAsync();
            var display = form.Files["display"];
            var original = form.Files["original"];
            bool gmOnly = form["gmOnly"] == "true";

            if (display is null) return Results.BadRequest("No image was sent.");
            if (display.Length > MaxDisplayBytes)
                return Results.BadRequest($"That picture is too large (limit {MaxDisplayBytes / 1024 / 1024} MB after resizing).");
            if (!AllowedTypes.Contains(display.ContentType))
                return Results.BadRequest($"{display.ContentType} is not an allowed image type. Use PNG, JPEG, WebP or GIF — not SVG.");

            var displayBytes = await ReadAsync(display);
            if (!LooksLikeImage(displayBytes))
                return Results.BadRequest("That file is not an image, whatever its name says.");

            byte[]? originalBytes = null;
            string? originalType = null;
            if (original is not null && original.Length > 0)
            {
                if (original.Length > MaxOriginalBytes)
                    return Results.BadRequest($"The original is too large (limit {MaxOriginalBytes / 1024 / 1024} MB).");
                if (!AllowedTypes.Contains(original.ContentType))
                    return Results.BadRequest($"{original.ContentType} is not an allowed image type.");

                originalBytes = await ReadAsync(original);
                if (!LooksLikeImage(originalBytes))
                    return Results.BadRequest("That file is not an image, whatever its name says.");
                originalType = original.ContentType;
            }

            var row = new SessionNoteImage
            {
                GmOnly = gmOnly,
                ContentType = display.ContentType,
                Data = displayBytes,
                Width = int.TryParse(form["width"], out var w) ? w : 0,
                Height = int.TryParse(form["height"], out var h) ? h : 0,
                OriginalContentType = originalType,
                OriginalData = originalBytes,
                FileName = Path.GetFileName(display.FileName),
            };
            db.SessionNoteImages.Add(row);
            await db.SaveChangesAsync();

            return Results.Ok(new { id = row.Id, url = $"{PathPrefix}/{row.Id}" });
        })
        // Antiforgery is deliberately NOT disabled: a form post through a minimal API is
        // validated by UseAntiforgery, and the editor sends the token the page rendered with
        // <AntiforgeryToken />. Turning it off would let another site drive this upload with
        // the GM's cookie on any browser whose SameSite handling is laxer than expected.
        .RequireAuthorization(p => p.RequireRole(Roles.Gamemaster));

        // ── Delivery ──────────────────────────────────────────────────────────
        // `full` is taken as a string rather than a bool: model binding rejects "?full=1" with a
        // 400 before the handler is reached, and 1 is what anyone writing such a link types.
        app.MapGet($"{PathPrefix}/{{id:int}}", async (int id, string? full, HttpContext ctx, AppDbContext db) =>
        {
            bool wantOriginal = full is "1" or "true" or "yes";

            var img = await db.SessionNoteImages.AsNoTracking()
                .FirstOrDefaultAsync(i => i.Id == id);
            if (img is null) return Results.NotFound();

            // A picture from the GM-only field is answered with 404 rather than 403 for anyone
            // else: 403 would confirm that an image with this id exists, which is itself a leak.
            if (img.GmOnly && !ctx.User.IsInRole(Roles.Gamemaster)) return Results.NotFound();

            bool wantFull = wantOriginal && img.OriginalData is not null;
            var bytes = wantFull ? img.OriginalData! : img.Data;
            var type  = wantFull ? img.OriginalContentType! : img.ContentType;

            // The bytes behind an id never change, so this can be cached hard. Private, because
            // a GM-only picture must not be held by anything between here and the browser.
            ctx.Response.Headers.CacheControl = "private, max-age=31536000, immutable";
            return Results.File(bytes, type, enableRangeProcessing: true,
                                entityTag: new Microsoft.Net.Http.Headers.EntityTagHeaderValue(
                                    $"\"{img.Id}-{(wantFull ? "f" : "d")}-{bytes.Length}\""));
        })
        .RequireAuthorization();
    }

    private static async Task<byte[]> ReadAsync(IFormFile file)
    {
        using var ms = new MemoryStream();
        await file.CopyToAsync(ms);
        return ms.ToArray();
    }
}
