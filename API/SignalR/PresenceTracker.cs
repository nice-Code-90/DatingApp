using System.Collections.Concurrent;

namespace API.SignalR;

public class PresenceTracker
{
    private static readonly ConcurrentDictionary<string,
     ConcurrentDictionary<string, byte>> OnLineUsers = new();

    public Task UserConnected(string userId, string connectionId)
    {
        var connections = OnLineUsers.GetOrAdd(userId, _ =>
            new ConcurrentDictionary<string, byte>());
        connections.TryAdd(connectionId, 0);
        return Task.CompletedTask;
    }

    public Task UserDisconnected(string userId, string connectionId)
    {
        if (OnLineUsers.TryGetValue(userId, out var connections))
        {
            connections.TryRemove(connectionId, out _);

            if (connections.IsEmpty)
            {
                OnLineUsers.TryRemove(userId, out _);
            }
        }
        return Task.CompletedTask;
    }

    public Task<string[]> GetOnlineUsers()
    {
        return Task.FromResult(OnLineUsers.Keys.OrderBy(k => k).ToArray());
    }

}
