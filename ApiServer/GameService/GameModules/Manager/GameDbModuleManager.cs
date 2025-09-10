using ApiServer.GameService.Models;
using ServerFramework.SqlServerServices.Models;

namespace ApiServer.GameService.GameModules.Manager;

public class GameDbModuleManager : IDisposable
{
    private readonly Dictionary<Type, IGameModule> _modules = [];
    
    private readonly long _accountId;
    
    private readonly SqlServerDbInfo _masterDbInfo;
    private readonly SqlServerDbInfo _slaveDbInfo;
    
    public GameDbModuleManager(long accountId, SqlServerDbInfo master, SqlServerDbInfo slave)
    {
        _masterDbInfo = master;
        _slaveDbInfo = slave;
        _accountId = accountId;
    }

    public T GetModule<T>() where T : class, IGameModule
    {
        if (_modules.TryGetValue(typeof(T), out var module) == true) 
            return module as T;
        
        var newModule = GameDbModuleFactory.CreateModule(typeof(T), _accountId, _masterDbInfo, _slaveDbInfo);
        _modules[typeof(T)] = newModule;
            
        return newModule as T;

    }

    public void Dispose()
    {
        foreach (var (_, module) in _modules)
        {
            module.Dispose();
        }
    }
}