using ApiServer.Handlers.Models;
using ApiServer.Services;
using ApiServer.Utils;
using DbContext.SharedContext;
using NetworkProtocols.WebApi;
using ServerFramework.CommonUtils.Helper;
using ServerFramework.SqlServerServices.Models;

namespace ApiServer.Handlers;

public abstract class BaseHandler : IDisposable
{
    protected LoggerService _loggerService;
    private SharedDbContext _sharedDbContext;

    private readonly Dictionary<string, IGameModule> _modules = [];
    public abstract Task InitializeModulesAsync(SqlServerDbInfo masterDbInfo, SqlServerDbInfo slaveDbInfo);

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

    protected void _AddModule(string moduleName, IGameModule newModule)
    {
        if (_modules.TryGetValue(moduleName, out _) == true)
            return;
        
        _modules[moduleName] = newModule;
    }

    protected T GetModule<T>() where T : class, IGameModule
    {
        if (_modules.TryGetValue(typeof(T).Name, out _) == false)
            throw new ApiServerException(ResultCode.SystemError, $"[{typeof(T).Name}] module not found]");

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