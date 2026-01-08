using System.Collections.Concurrent;
using ServerFramework.CommonUtils.DateTimeHelper;
using WorldServer.Network;

namespace WorldServer.GameService;

public class WorldService : IDisposable
{
    private readonly ConcurrentDictionary<string, WorldInstance> _worldInstances = new();
    private readonly CancellationTokenSource _cts = new();
    private bool _isTicking = false;

    public async Task<WorldInstance> CreateWorldInstance(string roomId, UserSessionInfo userSessionInfo)
    {
        var newWorldInstance = new WorldInstance(roomId, userSessionInfo);
        await newWorldInstance.InitializeAsync();
        if (_worldInstances.TryAdd(roomId, newWorldInstance) == false)
            return null;

        return newWorldInstance;
    }

    public WorldInstance GetWorldInstance(string roomId)
    {
        return _worldInstances.TryGetValue(roomId, out var instance) ? instance : null;
    }

    public void RemoveWorldInstance(string roomId)
    {
        var worldInstance = GetWorldInstance(roomId);
        if (worldInstance == null)
        {
            return;
        }

        worldInstance.Dispose();
        _worldInstances.TryRemove(roomId, out _);
    }

    public void StartGlobalTicker()
    {
        if (_isTicking == true)
            return;
        
        _isTicking = true;
        _ = Task.Run(_TickLoopAsync);
    }

    private async Task _TickLoopAsync()
    {
        while (_cts.IsCancellationRequested == false)
        {
            var startTime = TimeZoneHelper.UtcNow;
            foreach (var worldInstance in _worldInstances.Values)
            {
                worldInstance.Tick();
            }
            
            var elapsed = DateTime.UtcNow - startTime;
            var delay = Math.Max(0, 100 - (int)elapsed.TotalMilliseconds);
            await Task.Delay(delay, _cts.Token);
        }
    }

    public void Dispose()
    {
        _cts?.Cancel();
        _cts?.Dispose();
    }
}
    