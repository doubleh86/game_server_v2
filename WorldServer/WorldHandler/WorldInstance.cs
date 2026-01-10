using System.Collections.Concurrent;
using System.Net;
using System.Numerics;
using DbContext.GameDbContext;
using Microsoft.Identity.Client;
using MySqlDataTableLoader.Models;
using MySqlDataTableLoader.Utils.Helper;
using NetworkProtocols.Socket.WorldServerProtocols.GameProtocols;
using ServerFramework.CommonUtils.Helper;
using SuperSocket.Server.Abstractions;
using WorldServer.GameObjects;
using WorldServer.JobModels;
using WorldServer.Network;
using WorldServer.Services;
using WorldServer.WorldHandler.WorldDataModels;

namespace WorldServer.WorldHandler;

public partial class WorldInstance : IDisposable
{
    private readonly IGameDbContext _dbContext; // Read 용 dbContext 
    private readonly string _roomId;
    private readonly ConcurrentQueue<Job> _jobQueue = new();
    private int _isProcessing;
    private bool _isDisposed = false;
    
    private readonly LoggerService _loggerService;
    private readonly GlobalDbService _globalDbService;
    
    private readonly Dictionary<GameCommandId, Func<byte[], ValueTask>> _commandHandlers = new();
    private PlayerObject _worldOwner;

    public string GetRoomId() => _roomId;
    private UserSessionInfo _GetUserSessionInfo() => _worldOwner.GetSessionInfo();
    private WorldMapInfo _worldMapInfo;

    public WorldInstance(string roomId, LoggerService loggerService, GlobalDbService dbService)
    {
        _roomId = roomId;
        _loggerService = loggerService;
        _globalDbService = dbService;

        _dbContext = GameDbContextWrapper.Create();
        _RegisterGameHandler();
        
    }
    
    private void _RegisterGameHandler()
    {
        _commandHandlers.Add(GameCommandId.MoveCommand, _HandleMove);
        _commandHandlers.Add(GameCommandId.UseItemCommand, _HandleItemUse);
    }
    
    public async ValueTask InitializeAsync(UserSessionInfo sessionInfo)
    {
        await _InitializeWorldAsync(sessionInfo);
    }

    private async ValueTask _InitializeWorldAsync(UserSessionInfo sessionInfo)
    {
        _worldOwner = new PlayerObject(sessionInfo.Identifier, new Vector3(0, 0, 0), sessionInfo, 0);
        _worldOwner.UpdatePosition(new Vector3(10, 29, 30), 101);
        
        var playerInfo = await _dbContext.GetPlayerInfoAsync(1);
        _worldOwner.SetPlayerInfo(playerInfo);

        await _RoadCurrentWorldMapAsync(1);
    }

    private ValueTask _RoadCurrentWorldMapAsync(int worldId)
    {
        var worldMapInfo = MySqlDataTableHelper.GetData<WorldInfo>(worldId);
        if (worldMapInfo == null)
            return ValueTask.CompletedTask;

        _worldMapInfo = new WorldMapInfo(worldId, _worldOwner.AccountId);
        _worldMapInfo.Initialize();

        _worldMapInfo.AddObject(_worldOwner);
        
        return ValueTask.CompletedTask;
    }

    public void Tick()
    {
        if(IsAliveWorld() == false) 
            return;
        
        var centerCell = _worldMapInfo.GetCell(_worldOwner.GetZoneId(), _worldOwner.GetPosition());
        if (centerCell == null)
            return;
        
        var nearByCells = _worldMapInfo.GetNearByCells(_worldOwner.GetZoneId(), 
                                                                    centerCell.X, centerCell.Z, 
                                                                    range: 2);
        
        _Push(new MonsterUpdateJob(_worldOwner.GetPosition(), nearByCells, _OnMonsterUpdate, _loggerService));
    }

    public async ValueTask HandleGameCommand(GameCommandId command, byte[] commandData)
    {
        try
        {
            if (_commandHandlers.TryGetValue(command, out var handler) == false)
                return;
            
            await handler(commandData);
        }
        catch (Exception e)
        {
            _loggerService.Warning($"Command failed [{e.Message}][{command}]", e);
        }
    }

    private void _Push(Job action)
    {
        _jobQueue.Enqueue(action);
        _ProcessJobs();
    }
    
    private void _ProcessJobs()
    {
        if (_isDisposed)
            return;
        if (Interlocked.CompareExchange(ref _isProcessing, 1, 0) != 0)
            return;

        Task.Run(async () =>
        {
            try
            {
                while (_jobQueue.TryDequeue(out var job))
                {
                    try
                    {
                        await job.ExecuteAsync();
                    }
                    catch (Exception e)
                    {
                        _loggerService.Warning($"Job failed [{e.Message}]", e);    
                    }
                }
            }
            finally
            {
                Interlocked.Exchange(ref _isProcessing, 0);
                
                if(_jobQueue.IsEmpty == false)
                    _ProcessJobs();
            }
        });
    }

    public bool IsAliveWorld()
    {
        if (_isDisposed == true)
            return false;
        
        if (_worldOwner == null)
            return false;
        
        var session = _worldOwner.GetSessionInfo();
        if(session == null)
            return false;

        if (session.State == SessionState.Closed || session.State == SessionState.None)
            return false;
        
        return true;
    }

    private void _AutoSaveWorldState()
    {
        if (_worldOwner == null)
            return;
        
        _globalDbService.PushJob(_worldOwner.AccountId, async (dbContext) =>
        {
            // TODO : world state update
        });
    }

    public void Dispose()
    {
        if(_isDisposed) 
            return;

        _isDisposed = true;
        _AutoSaveWorldState();
        
        _jobQueue.Clear();
        _commandHandlers.Clear();
        _worldOwner = null;
        _worldMapInfo?.Dispose();
        _worldMapInfo = null;
        
        _dbContext?.Dispose();
    }
}