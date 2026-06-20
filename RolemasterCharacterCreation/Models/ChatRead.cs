namespace RolemasterCharacterCreation.Models;

// Per-user read marker for one conversation, used to compute unread badges. One row per
// (UserId, ConversationKey) where ConversationKey is "party" or "dm:{otherUserId}".
public class ChatRead
{
    public int Id { get; set; }

    public string UserId { get; set; } = "";

    public string ConversationKey { get; set; } = "";

    public DateTimeOffset LastReadAt { get; set; }
}
