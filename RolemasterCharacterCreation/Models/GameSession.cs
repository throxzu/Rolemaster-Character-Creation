namespace RolemasterCharacterCreation.Models;

// A player's availability response for a planned session.
public enum AttendanceResponse { Yes = 1, No = 2, Maybe = 3 }

// A game session the GM has planned on a specific calendar date. Players RSVP via
// SessionAttendance; everyone can see the full roster of responses.
public class GameSession
{
    public int Id { get; set; }

    public DateOnly Date { get; set; }

    // Optional start time of day.
    public TimeOnly? StartTime { get; set; }

    public string? Title { get; set; }
    public string? Note { get; set; }

    // GM can cancel a session without deleting it (keeps it visible, struck through).
    public bool Cancelled { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<SessionAttendance> Attendances { get; set; } = [];
}

// One player's response to one planned session. Unique per (GameSessionId, UserId).
public class SessionAttendance
{
    public int Id { get; set; }

    public int GameSessionId { get; set; }
    public GameSession GameSession { get; set; } = null!;

    // AspNetUsers.Id of the responding user.
    public required string UserId { get; set; }

    public AttendanceResponse Response { get; set; }

    public DateTime RespondedAt { get; set; } = DateTime.UtcNow;
}
