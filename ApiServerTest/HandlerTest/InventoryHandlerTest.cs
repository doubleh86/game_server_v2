using ApiServer.GameService.GameModules;
using ApiServer.GameService.Handlers.GameHandlers;
using ApiServer.Services;
using DbContext.SharedContext;
using DbContext.SharedContext.DbResultModel;

namespace ApiServerTest.HandlerTest;

public class InventoryHandlerTest : HandlerTestBase
{
    
    [Test]
    public async Task UseItemTest()
    {
        if (_accountDbResult == null)
        {
            Console.WriteLine("AccountDbResult is null");
            return;
        }
        
        var handler = new InventoryHandler(_accountDbResult.AccountId, _apiServerService, _eventService);
        await handler.InitializeModulesAsync(_accountDbResult.GetMainDbInfo(), _accountDbResult.GetMainDbInfo(true), true);
        
        var result = await handler.UseInventoryItemAsync(1, 10);
    }
    
}