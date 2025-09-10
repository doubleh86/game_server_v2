using ApiServer.GameService.Models;
using ServerFramework.SqlServerServices.Models;

namespace ApiServer.GameService.GameModules.Manager;

public class GameDbModuleManager : IDisposable
{
    private readonly Dictionary<string, IGameModule> _modules = [];
    
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
        var name = typeof(T).Name;
        if (_modules.TryGetValue(name, out var module) == true) 
            return module as T;
        
        var newModule = GameDbModuleFactory.CreateModule(name, _accountId, _masterDbInfo, _slaveDbInfo);
        _modules[name] = newModule;
            
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