using System.Text;
using System.Text.Json;
using NetworkProtocols.WebApi;
using NetworkProtocols.WebApi.Commands;
using NetworkProtocols.WebApi.Commands.Auth;
using NetworkProtocols.WebApi.Commands.User;

namespace ApiServerTest.ApiTest;

public class AuthTest : WebSendTestBase
{
    
    
    [Test, Order(1)]
    public async Task GetUserInfo()
    {
        _loginInfo.Sequence += 1;
        var request = new GetUserCommand.Request
        {
            AccountId = _loginInfo.AccountId,
            Sequence = _loginInfo.Sequence,
            SubSequence = _loginInfo.SubSequence,
            Token = _loginInfo.Token,
        };
        
        var response = await ApiTestHelper.SendPacket<GetUserCommand.Request, GetUserCommand.Response>(request, LoginInfo.ServerUrl);
        if (response.ResultCode != (int)GameResultCode.Ok)
        {
            Console.WriteLine($"Error Message : {response.DebugMessage}");
            return;
        }
        
        Console.WriteLine(JsonSerializer.Serialize(response));
    }

}