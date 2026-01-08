using System.Collections.Concurrent;
using WorldServer.GameService;
using WorldServer.Network;

namespace WorldServer.Services;

public class WorldService : IDisposable
{
    private WorldServerService _serverService;
    
    private readonly ConcurrentDictionary<string, WorldInstance> _worldInstances = new();
    private readonly int _workerCount = Environment.ProcessorCount;
    private readonly CancellationTokenSource _cts = new();

    public void SetServerService(WorldServerService serverService)
    {
        _serverService = serverService;
    }
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
        for(int i = 0; i < _workerCount; i++)
        {
            int shardIndex = i;
            Task.Factory.StartNew(() => GlobalTickLoop(shardIndex), TaskCreationOptions.LongRunning);
        }
    }

    private async Task GlobalTickLoop(int shardIndex)
    {
        var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(30));
        while (await timer.WaitForNextTickAsync(_cts.Token))
        {
            try
            {
                foreach (var worldInstance in _worldInstances.Values)
                {
                    if (Math.Abs(worldInstance.GetRoomId().GetHashCode() % _workerCount) == shardIndex)
                    {
                        worldInstance.Tick();
                    }
                }
            }
            catch (Exception e)
            {
                _serverService.GetLoggerService().Warning($"GlobalTickLoop Error : {e.Message}", e);
            }
        }
    }

    public void Dispose()
    {
        _cts?.Cancel();
        _cts?.Dispose();
    }
}
    