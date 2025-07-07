using ApiServer.Services;
using DbContext.SharedContext;
using ServerFramework.CommonUtils.Helper;

namespace ApiServer.Handlers;

public abstract class BaseHandler : IDisposable
{
    private readonly ApiServerService _serverService;
    protected LoggerService _loggerService;
    
    private SharedDbContext _sharedDbContext;

    protected BaseHandler(ApiServerService serverService)
    {
        _serverService = serverService;
        _loggerService = _serverService.LoggerService;
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
    }
}