using ApiServer.GameService.GameModules;
using ApiServer.GameService.Models;
using ApiServer.Services;
using ApiServer.Utils;
using DbContext.SharedContext;
using NetworkProtocols.WebApi;
using ServerFramework.CommonUtils.Helper;
using ServerFramework.SqlServerServices.Models;

namespace ApiServer.GameService.Handlers.GameHandlers;

public abstract class BaseHandler : IDisposable
{
    protected readonly long _accountId;
    protected LoggerService _loggerService;
    private SharedDbContext _sharedDbContext;

    private readonly Dictionary<string, IGameModule> _modules = [];
    // public abstract Task InitializeModulesAsync(SqlServerDbInfo masterDbInfo, SqlServerDbInfo slaveDbInfo);

    protected BaseHandler(long accountId, ApiServerService serverService)
    {
        _accountId = accountId;
        _loggerService = serverService.LoggerService;
    }

    public virtual Task InitializeModulesAsync(SqlServerDbInfo masterDbInfo, SqlServerDbInfo slaveDbInfo)
    {
        var gameUserModule = new GameUserModule(_accountId, masterDbInfo, slaveDbInfo);
        _AddModule(nameof(GameUserModule), gameUserModule);
        
        return Task.CompletedTask;
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