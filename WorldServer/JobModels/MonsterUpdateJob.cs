using System.Collections.Concurrent;
using WorldServer.GameObjects;

namespace WorldServer.JobModels;

public class MonsterUpdateJob(ConcurrentDictionary<long,MonsterObject> monsterObject, Func<ValueTask> action) : Job(null)
{
    private readonly Func<ValueTask> _action = action;
    private readonly ConcurrentDictionary<long, MonsterObject> _monsterObject = monsterObject;
    public override void Execute()
    {
        
    }
    
    public override async ValueTask ExecuteAsync()
    {
        int count = 0;
        foreach (var (_, monster) in _monsterObject)
        {
            await monster.UpdateStateAsync(new Random().Next(0, 3));    
            count++;

            if (count % 50 == 0)
                await Task.Yield();
        }
        
        await _action();
    }
}