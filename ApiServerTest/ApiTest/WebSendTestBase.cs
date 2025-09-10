using NetworkProtocols.WebApi;
using NetworkProtocols.WebApi.Commands.Auth;

namespace ApiServerTest.ApiTest;

public abstract class WebSendTestBase
{
    protected readonly LoginInfo _loginInfo = new();
    
    [OneTimeSetUp]
    public async Task SetUp()
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
}