using System.Collections.Concurrent;
using System.Numerics;
using NetworkProtocols.Socket.WorldServerProtocols.GameProtocols;
using ServerFramework.CommonUtils.Helper;
using WorldServer.GameObjects;
using WorldServer.JobModels;
using WorldServer.Network;

namespace WorldServer.WorldHandler;

public partial class WorldInstance : IDisposable
{
    private readonly string _roomId;
    private readonly ConcurrentQueue<Job> _jobQueue = new();
    private int _isProcessing;
    
    private readonly ConcurrentDictionary<long, MonsterObject> _monsters = new();
    private readonly LoggerService _loggerService;
    
    private readonly Dictionary<GameCommandId, Func<byte[], ValueTask>> _commandHandlers = new();
    private PlayerObject _worldOwner;

    public string GetRoomId() => _roomId;
    private UserSessionInfo _GetUserSessionInfo() => _worldOwner.GetSessionInfo();

    public WorldInstance(string roomId, LoggerService loggerService)
    {
        _roomId = roomId;
        _loggerService = loggerService;
        
        _RegisterGameHandler();
        
    }
    
    private void _RegisterGameHandler()
    {
        _commandHandlers.Add(GameCommandId.MoveCommand, _HandleMove);
    }
    
    public async ValueTask InitializeAsync(UserSessionInfo sessionInfo)
    {
        await _InitializeWorldAsync(sessionInfo);
        var tasks = Enumerable.Range(0, 100).Select(async i =>
        {
            // Monster 생성 및 추가 (비동기 시뮬레이션이 필요하다면 여기서 수행)
            var monster = new MonsterObject(i, new Vector3(10, 10, 10));
            _monsters.TryAdd(i, monster);
            
            await Task.Yield(); // 다른 작업에 실행권을 잠시 양보 (대량 작업 시 UI/네트워크 스레드 방해 방지)
        });
        
        // 2. 모든 작업이 완료될 때까지 비동기로 대기합니다.
        await Task.WhenAll(tasks);
    }

    private async ValueTask _InitializeWorldAsync(UserSessionInfo sessionInfo)
    {
        // Maybe Db Call?
        _worldOwner = new PlayerObject(sessionInfo.Identifier, new Vector3(0, 0, 0), sessionInfo);
        _worldOwner.UpdatePosition(new Vector3(10, 29, 30));
        // World 생성 NPC 생성
    }

    public void Tick()
    {
        _Push(new MonsterUpdateJob(_monsters, _OnMonsterUpdate));
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

    public void Dispose()
    {
        // TODO release managed resources here
    }
}