using System.Collections.Concurrent;
using WorldServer.Network;

namespace WorldServer.Services;

public class UserService
{
    private long _currentUserId;
    private readonly ConcurrentDictionary<long, UserSessionInfo> _sessions = new();

    public void AddUser(UserSessionInfo info)
    {
        Interlocked.Increment(ref _currentUserId);
        info.SetIdentifier(_currentUserId);
        
        _sessions.TryAdd(info.Identifier, info);
    }

    public void RemoveUser(long identifier)
    {
        _sessions.TryRemove(identifier, out _);
    }
}