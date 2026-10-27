namespace RolemasterCharacterCreation.Models;

// Notes for a single game session. PublicNotes are visible to players on the
// read-only "Session Notes" page; GmNotes are only ever shown to the GM.
public class SessionNote
{
    public int Id { get; set; }

    // The in-real-life date the session was played.
    public DateTime SessionDate { get; set; } = DateTime.Today;

    public required string Title { get; set; }

    // Recap / information the players are allowed to read.
    public string? PublicNotes { get; set; }

    // Secret notes for the GM's eyes only.
    public string? GmNotes { get; set; }

    // True once the notes above hold rich text from the editor. Notes written before the
    // editor existed are plain text and must keep rendering as such — sniffing for a '<'
    // instead would mangle a note that legitimately says "a < b".
    public bool NotesAreHtml { get; set; }

    // Pictures pasted into either field. Deleting the note deletes them with it.
    public List<SessionNoteImage> Images { get; set; } = [];

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
