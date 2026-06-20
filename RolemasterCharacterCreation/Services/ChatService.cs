using System.Collections.Concurrent;
using RolemasterCharacterCreation.Models;

namespace RolemasterCharacterCreation.Services;

// Singleton in-memory broker for the chat feature. It only does pub/sub and presence —
// message persistence is the responsibility of the caller's scoped AppDbContext, so this
// singleton never touches the database (avoiding DbContext lifetime issues).
//
// Each open chat page runs on its own InteractiveServer circuit and subscribes to
// MessagePosted/PresenceChanged; the broker just fans events out to them.
public sealed class ChatService
{
    // Raised after a message has been persisted, so every open chat circuit can react.
    public event Action<ChatMessage>? MessagePosted;

    // Raised whenever someone connects or disconnects, so presence dots update live.
    public event Action? PresenceChanged;

    // userId -> number of open chat circuits for that user (multiple tabs/windows).
    private readonly ConcurrentDictionary<string, int> _online = new();

    public void Publish(ChatMessage message) => MessagePosted?.Invoke(message);

    public void Connect(string userId)
    {
        _online.AddOrUpdate(userId, 1, (_, count) => count + 1);
        PresenceChanged?.Invoke();
    }

    public void Disconnect(string userId)
    {
        // Decrement, and drop the user entirely once their last circuit closes.
        var remaining = _online.AddOrUpdate(userId, 0, (_, count) => count - 1);
        if (remaining <= 0)
            _online.TryRemove(userId, out _);

        PresenceChanged?.Invoke();
    }

    public bool IsOnline(string userId) => _online.TryGetValue(userId, out var c) && c > 0;
}
