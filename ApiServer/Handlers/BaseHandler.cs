using ApiServer.Handlers.Models;
using ApiServer.Services;
using DbContext.SharedContext;
using ServerFramework.CommonUtils.Helper;
using ServerFramework.SqlServerServices.Models;

namespace ApiServer.Handlers;

public abstract class BaseHandler : IDisposable
{
    protected LoggerService _loggerService;
    private SharedDbContext _sharedDbContext;

    private readonly Dictionary<string, IGameModule> _modules = [];

    protected BaseHandler(ApiServerService serverService)
    {
        _loggerService = serverService.LoggerService;
    }

    protected SharedDbContext _GetSharedDbContext()
    {
        if(_sharedDbContext == null)
            _sharedDbContext = SharedDbContext.Create();
        
        return _sharedDbContext;
    }
    
    public virtual Task InitializeModulesAsync(SqlServerDbInfo masterDbInfo, SqlServerDbInfo slaveDbInfo)
    {
        return Task.CompletedTask;
    }

    public virtual void InitializeModules(SqlServerDbInfo masterDbInfo, SqlServerDbInfo slaveDbInfo)
    {
        
    }

    protected void _AddModule(string moduleName, IGameModule newModule)
    {
        if (_modules.TryGetValue(moduleName, out _) == true)
            return;
        
        _modules[moduleName] = newModule;
    }

    protected T GetModule<T>() where T : class, IGameModule
    {
        if (_modules.TryGetValue(typeof(T).Name, out _) == false)
            return null;

        return _modules[typeof(T).Name] as T;
    }
    
    public void Dispose()
    {
        _sharedDbContext?.Dispose();
        foreach (var (_, module) in _modules)
        {
            module.Dispose();
        }
    }
}