using ApiServer.GameService.Handlers.GameHandlers;

namespace ApiServerTest.HandlerTest;

public class MailHandlerTest : HandlerTestBase
{
    [Test]
    public async Task InsertMailItemTest()
    {
        if (_accountDbResult == null)
        {
            Console.WriteLine("AccountDbResult is null");
            return;
        }

        var handler = new MailHandler(_accountDbResult.AccountId, _apiServerService);
        await handler.InitializeModulesAsync(_accountDbResult.GetMainDbInfo(), _accountDbResult.GetMainDbInfo(true), true);

        await handler.InsertMailItemForTestAsync();
    }
}