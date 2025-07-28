using NetworkProtocols.WebApi;
using NetworkProtocols.WebApi.Commands.Auth;
using NetworkProtocols.WebApi.Commands.Inventory;

namespace ApiServerTest.ApiTest;

public class InventoryTest
{
    private readonly LoginInfo _loginInfo = new();
    
    [Test, Order(0)]
    public async Task Login()
    {
        var request = new AuthCommand.Request
        {
            LoginId = _loginInfo.LoginId,
        };

        var response = await ApiTestHelper.SendPacket<AuthCommand.Request, AuthCommand.Response>(request, LoginInfo.ServerUrl);
        if (response.ResultCode != (int)GameResultCode.Ok)
        {
            Console.WriteLine($"Error Message : {response.DebugMessage}");
            return;
        }
        _loginInfo.AccountId = response.AccountId;
        _loginInfo.Token = response.Token;
    }

    [Test, Order(1)]
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