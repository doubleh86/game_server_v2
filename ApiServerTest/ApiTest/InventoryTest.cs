using NetworkProtocols.WebApi;
using NetworkProtocols.WebApi.Commands.Auth;
using NetworkProtocols.WebApi.Commands.Inventory;

namespace ApiServerTest.ApiTest;

public class InventoryTest : WebSendTestBase
{
    [Test, Order(0)]
    public async Task UseInventoryItemTest()
    {
        _loginInfo.Sequence += 1;
        var request = new ItemUseCommand.Request
        {
            AccountId = _loginInfo.AccountId,
            Sequence = _loginInfo.Sequence,
            SubSequence = _loginInfo.SubSequence,
            Token = _loginInfo.Token,

            ItemIndex = 1,
            Quantity = 2
        };
        
        var response = await ApiTestHelper.SendPacket<ItemUseCommand.Request, ItemUseCommand.Response>(request, LoginInfo.ServerUrl);
        if (response.ResultCode != (int)GameResultCode.Ok)
        {
            Console.WriteLine($"Error Message : {response.DebugMessage}");
        }
    }
}