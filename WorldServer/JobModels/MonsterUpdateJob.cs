using System.Collections.Concurrent;
using WorldServer.GameObjects;
using WorldServer.WorldHandler.WorldDataModels;

namespace WorldServer.JobModels;

//ㅜ
public class MonsterUpdateJob(List<MapCell> nearByCells, Func<List<MonsterObject>, ValueTask> action) : Job(null)
{
    private readonly Func<List<MonsterObject>, ValueTask> _action = action;
    private readonly List<MapCell> _nearByCells = nearByCells;
    public override void Execute()
    {
        
    }
    
    public override async ValueTask ExecuteAsync()
    {
        List<MonsterObject> updateMonsters = new();
        foreach (var cell in _nearByCells)
        {
            // Monster 찾아서 Update
            foreach (var (_, obj) in cell.GetMapObjects())
            {
                if (obj is not MonsterObject monster)
                    continue;
                
                var result = await monster.UpdateStateAsync(new Random().Next(0, 3));
                if (result == false)
                    continue;
                
                updateMonsters.Add(monster);
            }
        }
        
        await _action(updateMonsters);
    }
}