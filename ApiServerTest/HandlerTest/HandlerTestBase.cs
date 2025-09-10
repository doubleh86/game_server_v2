using ApiServer.GameService.Handlers.GameHandlers;
using ApiServer.Services;
using DbContext.SharedContext;
using DbContext.SharedContext.DbResultModel;
using ServerFramework.CommonUtils.DateTimeHelper;
using ServerFramework.CommonUtils.Helper;

namespace ApiServerTest.HandlerTest;

public class HandlerTestBase : IDisposable, IAsyncDisposable
{
    protected readonly string _platformId = "test0101";
    protected GetAccountDbResult _accountDbResult;
    protected ApiServerService _apiServerService;
    protected EventService _eventService;
    
    protected ConfigurationHelper _configurationHelper;
    
    [OneTimeSetUp]
    public async Task Setup()
    {
        _InitializeConfiguration();
        
        _apiServerService = new ApiServerService();
        _apiServerService.Initialize();
        
        _eventService = new EventService();
        using var dbContext = SharedDbContext.Create();

        var eventList = await dbContext.GetEventInfoListAsync();
        _eventService.Initialize(eventList ?? []);

        var authHandler = new AuthHandler(_apiServerService);
        _accountDbResult = await authHandler.GetAccountInfoAsync(_platformId);
    }

    private void _InitializeConfiguration()
    {
        var configFiles = new List<string> {"appsettings.json", "Settings/sqlSettings.json"};
        
        _configurationHelper = new ConfigurationHelper();
        _configurationHelper.Initialize(configFiles);
        var serviceTimeZone = _configurationHelper.GetValue("ServiceTimeZone", "UTC");
        
        TimeZoneHelper.Initialize(serviceTimeZone, dateTimeProvider: new FakeDateTimeProvider(DateTime.UtcNow));
    }
    
    public void Dispose()
    {
        _apiServerService?.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        if (_apiServerService != null) 
            await _apiServerService.DisposeAsync();
    }
}