namespace RolemasterCharacterCreation.Models;

// A picture pasted into a session note. Images live in the database rather than on disk for
// the same reason map data does: a database backup is then the whole campaign, and a redeploy
// cannot leave notes pointing at files that are no longer there.
public class SessionNoteImage
{
    public int Id { get; set; }

    // Null while the GM is still composing a new session: the picture can be pasted before
    // there is a note row to hang it on, and is adopted when that note is saved.
    public int? SessionNoteId { get; set; }
    public SessionNote? SessionNote { get; set; }

    // Pasted into the GM-only field, so the endpoint serving it demands the Gamemaster role.
    public bool GmOnly { get; set; }

    // What the note renders: re-encoded in the browser to at most 1600px on its long edge.
    public required string ContentType { get; set; }
    public required byte[] Data { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }

    // The file exactly as it was pasted, served from ?full=1 when the picture is clicked.
    // Null when the paste was already small enough to be its own display copy.
    public string? OriginalContentType { get; set; }
    public byte[]? OriginalData { get; set; }

    public string? FileName { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
