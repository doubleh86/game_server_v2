using NetworkProtocols.WebApi;
using NetworkProtocols.WebApi.Commands.Auth;
using NetworkProtocols.WebApi.Commands.Shop;

namespace ApiServerTest.ApiTest;

public class ShopTest : WebSendTestBase
{
    [Test, Order(0)]
    public async Task ShopBuy()
    {
        _loginInfo.Sequence += 1;
        var request = new ShopBuyCommand.Request
        {
            AccountId = _loginInfo.AccountId,
            Sequence = _loginInfo.Sequence,
            SubSequence = _loginInfo.SubSequence,
            Token = _loginInfo.Token,
            
            ItemIndex = 1,
            Amount = 10,
        };
        
        var response = await ApiTestHelper.SendPacket<ShopBuyCommand.Request, ShopBuyCommand.Response>(request, LoginInfo.ServerUrl);
        if (response.ResultCode != (int)GameResultCode.Ok)
        {
            Console.WriteLine($"Error Message : {response.DebugMessage}");
            return;
        }
    }

}