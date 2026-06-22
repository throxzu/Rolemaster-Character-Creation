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

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
