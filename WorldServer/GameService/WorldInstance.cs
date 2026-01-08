using System.Collections.Concurrent;
using System.Numerics;
using MessagePack;
using NetworkProtocols.Socket.WorldServerProtocols.GameProtocols;
using WorldServer.GameObjects;
using WorldServer.JobModels;
using WorldServer.Network;

namespace WorldServer.GameService;

public partial class WorldInstance(string roomId, UserSessionInfo userSessionInfo) : IDisposable
{
    private readonly string _roomId = roomId;
    private readonly ConcurrentQueue<Job> _jobQueue = new();
    private int _isProcessing = 0;
    
    private readonly ConcurrentDictionary<long, MonsterObject> _monsters = new();
    
    private UserSessionInfo _userSessionInfo = userSessionInfo;
    public string GetRoomId() => _roomId;

    public async ValueTask InitializeAsync()
    {
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

    public void Tick()
    {
        _Push(new MonsterUpdateJob(_monsters, _OnMonsterUpdate));
    }

    public async ValueTask HandleGameCommand(GameCommandId command, byte[] commandData)
    {
        switch (command)
        {
            case GameCommandId.MoveCommand:
                await _HandleMove(commandData);
                break;
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
                    await job.ExecuteAsync();
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