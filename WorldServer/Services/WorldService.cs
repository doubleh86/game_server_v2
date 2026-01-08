using System.Collections.Concurrent;
using WorldServer.Network;
using WorldInstance = WorldServer.WorldHandler.WorldInstance;

namespace WorldServer.Services;

public class WorldService : IDisposable
{
    private WorldServerService _serverService;
    private List<WorldInstance>[] _shardWorldLists;
    private object[] _shardLocks;
    
    private readonly ConcurrentDictionary<string, WorldInstance> _worldInstances = new();
    private readonly int _workerCount = Environment.ProcessorCount;
    private readonly CancellationTokenSource _cts = new();

    public void Initialize(WorldServerService serverService)
    {
        _shardWorldLists = new List<WorldInstance>[_workerCount];
        _shardLocks = new object[_workerCount];
        for (int i = 0; i < _workerCount; i++)
        {
            _shardWorldLists[i] = new List<WorldInstance>();
            _shardLocks[i] = new object();
        }
        
        _SetServerService(serverService);    
    }
    private void _SetServerService(WorldServerService serverService)
    {
        _serverService = serverService;
    }
    public async Task<WorldInstance> CreateWorldInstance(string roomId, UserSessionInfo userSessionInfo)
    {
        var newWorldInstance = new WorldInstance(roomId, _serverService.GetLoggerService(), 
                                                 _serverService.GetGlobalDbService());
        
        await newWorldInstance.InitializeAsync(userSessionInfo);
        
        if (_worldInstances.TryAdd(roomId, newWorldInstance) == false)
            return null;
        
        var shardIndex = Math.Abs(roomId.GetHashCode() % _workerCount);
        lock (_shardLocks[shardIndex])
        {
            _shardWorldLists[shardIndex].Add(newWorldInstance);
        }
        
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
            Task.Factory.StartNew(() => _GlobalTickLoop(shardIndex), TaskCreationOptions.LongRunning);
        }
    }

    private async Task _GlobalTickLoop(int shardIndex)
    {
        var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(30));
        var myShardList = _shardWorldLists[shardIndex];
        while (await timer.WaitForNextTickAsync(_cts.Token))
        {
            try
            {
                WorldInstance[] shapShot;
                lock (_shardLocks[shardIndex])
                {
                    foreach (var worldInstance in myShardList)
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
    