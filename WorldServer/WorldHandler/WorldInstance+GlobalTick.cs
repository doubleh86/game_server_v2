using WorldServer.JobModels;

namespace WorldServer.WorldHandler;

public partial class WorldInstance
{
    private static readonly TimeSpan _AutoSaveInterval = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan _MonsterUpdateInterval = TimeSpan.FromMilliseconds(100);
    
    private DateTime _lastCallAutoSaveTime = DateTime.UtcNow;
    private DateTime _lastCallMonsterUpdateTime = DateTime.UtcNow;
    private int _autoSaving;
    
    public void Tick()
    {
        if(IsAliveWorld() == false) 
            return;

        var currentTime = DateTime.UtcNow;
        _MonsterUpdateTick(currentTime);
        _AutoSaveTick(currentTime);
    }

    private void _AutoSaveTick(DateTime currentTime)
    {
        if (currentTime - _lastCallAutoSaveTime < _AutoSaveInterval)
            return;
        
        _lastCallAutoSaveTime = currentTime;
        _AutoSaveWorldState();
    }

    private void _MonsterUpdateTick(DateTime currentTime)
    {
        if (Volatile.Read(ref _isChangingWorld) == 1)
            return;
        
        if (currentTime - _lastCallMonsterUpdateTime < _MonsterUpdateInterval)
            return;
        
        _lastCallMonsterUpdateTime = currentTime;
        var centerCell = _worldMapInfo.GetCell(_worldOwner.GetPosition());
        if (centerCell == null)
            return;
        
        var nearByCells = _worldMapInfo.GetWorldNearByCells(_worldOwner.GetZoneId(), 
                                                            _worldOwner.GetPosition(), 
                                                            range: 2);
        
        _Push(new MonsterUpdateJob(_worldOwner.GetPosition(), nearByCells, _OnMonsterUpdate, _loggerService));
    }

}