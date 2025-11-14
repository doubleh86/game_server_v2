using ApiServer.GameService.GameModules;
using ApiServer.GameService.GameModules.Manager;
using ApiServer.GameService.Models;
using ApiServer.Services;
using ApiServer.Utils;
using ApiServer.Utils.GameUtils;
using DbContext.SharedContext;
using NetworkProtocols.WebApi;
using ServerFramework.CommonUtils.Helper;
using ServerFramework.SqlServerServices.Models;
using SharedDbContext = DbContext.SharedContext.SqlServerContext.SharedDbContext;

namespace ApiServer.GameService.Handlers.GameHandlers;

public abstract class BaseHandler : IDisposable
{
    protected readonly long _accountId;
    protected readonly LoggerService _loggerService;
    protected EventService _eventService;
    private SharedDbContext _sharedDbContext;
    private RefreshDataHelper _refreshDataHelper;
    
    public RefreshDataHelper RefreshDataHelper => _refreshDataHelper;
    private GameDbModuleManager _moduleManager;
    
    protected GameDbModuleManager _GetModuleManager() => _moduleManager;
    
    protected BaseHandler(long accountId, ApiServerService serverService, EventService eventService = null)
    {
        _accountId = accountId;
        _loggerService = serverService.LoggerService;
        _eventService = eventService;
    }

    public async Task InitializeModulesAsync(SqlServerDbInfo masterDbInfo, SqlServerDbInfo slaveDbInfo, bool isRefreshResponse)
    {
        _moduleManager = new GameDbModuleManager(_accountId, masterDbInfo, slaveDbInfo);

        if (isRefreshResponse == true)
        {
            _refreshDataHelper = new RefreshDataHelper();
            var userDbInfo = await _GetModule<GameUserModule>().GetGameUserDbModelAsync();
            _refreshDataHelper.SetGameUserInfo(userDbInfo);
        }
    }
    
    protected T _GetModule<T>()  where T : class, IGameModule
    {
        if(_moduleManager == null)
            throw new ApiServerException(GameResultCode.SystemError, $"Module manager does not initialize");

        return _moduleManager.GetModule<T>();
    }
    
    protected RefreshDataHelper _GetRefreshDataHelper()
    {
        if (_refreshDataHelper == null)
        {
            throw new ApiServerException(GameResultCode.SystemError, "RefreshDataHelper does not initialize");
        } 
        
        return _refreshDataHelper;
    }

    protected SharedDbContext _GetSharedDbContext()
    {
        if(_sharedDbContext == null)
            _sharedDbContext = SharedDbContext.Create();
        
        return _sharedDbContext;
    }

    
    public void Dispose()
    {
        _sharedDbContext?.Dispose();
        _moduleManager?.Dispose();
    }
}