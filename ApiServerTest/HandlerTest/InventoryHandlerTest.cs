using ApiServer.GameService.GameModules;
using ApiServer.GameService.Handlers.GameHandlers;
using ApiServer.Services;
using DbContext.SharedContext;
using DbContext.SharedContext.DbResultModel;

namespace ApiServerTest.HandlerTest;

public class InventoryHandlerTest : IDisposable, IAsyncDisposable
{
    private string _platformId = "test0101";
    private GetAccountDbResult _accountDbResult;
    private ApiServerService _apiServerService;
    private EventService _eventService;
    
    [SetUp]
    public async Task Setup()
    {
        _apiServerService = new ApiServerService();
        _apiServerService.Initialize();
        
        _eventService = new EventService();
        using var dbContext = SharedDbContext.Create();

        var eventList = await dbContext.GetEventInfoListAsync();
        _eventService.Initialize(eventList ?? []);

        var authHandler = new AuthHandler(_apiServerService);
        _accountDbResult = await authHandler.GetAccountInfoAsync(_platformId);
        
    }
    
    [Test]
    public async Task UseItemTest()
    {
        if (_accountDbResult == null)
        {
            Console.WriteLine("AccountDbResult is null");
            return;
        }
        
        var handler = new InventoryHandler(_accountDbResult.AccountId, _apiServerService, _eventService);
        await handler.InitializeModulesAsync(_accountDbResult.GetMainDbInfo(), _accountDbResult.GetMainDbInfo(true));
        
        var result = await handler.UseInventoryItemAsync(1, 10);
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