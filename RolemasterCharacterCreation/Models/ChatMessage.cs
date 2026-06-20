namespace RolemasterCharacterCreation.Models;

// A single chat message. SenderId/RecipientId are plain user-id strings with no FK so a
// deleted user's messages survive intact (the SenderName snapshot still shows who said it),
// mirroring how the audit log keeps a nullable ChangedByUserId.
public class ChatMessage
{
    public int Id { get; set; }

    public string SenderId { get; set; } = "";

    // DisplayName snapshot at send time, so old messages keep showing the original name.
    public string SenderName { get; set; } = "";

    // null => the shared Party channel; non-null => a 1:1 direct message to that user.
    public string? RecipientId { get; set; }

    public string Text { get; set; } = "";

    public DateTimeOffset SentAt { get; set; }
}
