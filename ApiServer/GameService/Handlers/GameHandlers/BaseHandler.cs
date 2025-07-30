using ApiServer.GameService.GameModules;
using ApiServer.GameService.Models;
using ApiServer.Services;
using ApiServer.Utils;
using ApiServer.Utils.GameUtils;
using DbContext.SharedContext;
using DbContext.SharedContext.DbResultModel;
using NetworkProtocols.WebApi;
using ServerFramework.CommonUtils.EventHelper;
using ServerFramework.CommonUtils.Helper;
using ServerFramework.SqlServerServices.Models;

namespace ApiServer.GameService.Handlers.GameHandlers;

public abstract class BaseHandler : IDisposable
{
    protected readonly long _accountId;
    protected readonly LoggerService _loggerService;
    protected EventService _eventService;
    private SharedDbContext _sharedDbContext;
    private RefreshDataHelper _refreshDataHelper;
    
    protected readonly Dictionary<string, IGameModule> _modules = [];
    public RefreshDataHelper RefreshDataHelper => _refreshDataHelper;
    
    protected BaseHandler(long accountId, ApiServerService serverService, EventService eventService = null)
    {
        _accountId = accountId;
        _loggerService = serverService.LoggerService;
        _eventService = eventService;
    }

    protected RefreshDataHelper _GetRefreshDataHelper()
    {
        if(_refreshDataHelper == null)
            _refreshDataHelper = new RefreshDataHelper();
        
        return _refreshDataHelper;
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
        if (_modules.TryGetValue(typeof(T).Name, out var module) == false)
            throw new ApiServerException(GameResultCode.SystemError, $"[{typeof(T).Name}] module not found]");

        return module as T;
    }
    
    public T GetModuleForTest<T>() where T : class, IGameModule
    {
        if (_modules.TryGetValue(typeof(T).Name, out var module) == false)
            throw new ApiServerException(GameResultCode.SystemError, $"[{typeof(T).Name}] module not found]");

        return module as T;
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