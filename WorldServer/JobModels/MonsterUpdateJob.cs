using System.Numerics;
using ServerFramework.CommonUtils.Helper;
using WorldServer.GameObjects;
using WorldServer.WorldHandler.WorldDataModels;

namespace WorldServer.JobModels;


public class MonsterUpdateJob : Job
{
    private readonly Func<List<MonsterObject>, ValueTask> _action;
    private readonly List<MapCell> _nearByCells;
    
    private readonly Vector3 _playerPosition;

    public MonsterUpdateJob(Vector3 playerPosition, List<MapCell> nearByCells, 
        Func<List<MonsterObject>, ValueTask> action, 
        LoggerService loggerService) : base(loggerService)
    {
        _nearByCells = nearByCells;
        _action = action;
        _playerPosition = playerPosition;
    }
    
    public override async ValueTask ExecuteAsync()
    {
        var targetMonsters = new List<MonsterObject>();
        foreach (var cell in _nearByCells)
        {
            foreach (var (_, obj) in cell.GetMapObjects())
            {
                if (obj is not MonsterObject monster)
                    continue;
                
                targetMonsters.Add(monster);
            }
        }

        if (targetMonsters.Count < 1)
            return;
        
        await Task.Run(() =>
        {
            try
            {
                Parallel.ForEach(targetMonsters, (monster) => { monster.UpdateAI(_playerPosition); });
            }
            catch (Exception e)
            {
                _loggerService.Warning("Error in MonsterUpdateJob", e);
            }
            
        });
        
        await _action(targetMonsters);
    }
}