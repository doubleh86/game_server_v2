using System.Text;
using System.Text.Json;
using NetworkProtocols.WebApi;
using NetworkProtocols.WebApi.Commands;
using NetworkProtocols.WebApi.Commands.Auth;
using NetworkProtocols.WebApi.Commands.User;

namespace ApiServerTest.ApiTest;

public class AuthTest
{
    private string _serverUrl = "http://127.0.0.1:1200";
    private string _loginId = "ccccc";
    private byte _sequence = 0;
    private byte _subSequence = 0;

    private long _accountId = 0;
    private string _token = string.Empty;

    [Test, Order(0)]
    public async Task Login()
    {
        var request = new AuthCommand.Request
        {
            LoginId = _loginId
        };

        var response = await _SendPacket<AuthCommand.Request, AuthCommand.Response>(request);
        if (response.ResultCode != (int)ResultCode.Ok)
        {
            Console.WriteLine($"Error Message : {response.DebugMessage}");
            return;
        }
        _accountId = response.AccountId;
        _token = response.Token;
    }

    [Test, Order(1)]
    public async Task GetUserInfo()
    {
        _sequence += 1;
        var request = new GetUserCommand.Request
        {
            AccountId = _accountId,
            Sequence = _sequence,
            SubSequence = _subSequence,
            Token = _token
        };
        
        var response = await _SendPacket<GetUserCommand.Request, GetUserCommand.Response>(request);
        if (response.ResultCode != (int)ResultCode.Ok)
        {
            Console.WriteLine($"Error Message : {response.DebugMessage}");
            return;
        }
    }

    private async Task<TResponse> _SendPacket<T, TResponse>(T request) where T : RequestBase
    {
        using var client = new HttpClient();
        using var requestMessage = new HttpRequestMessage(HttpMethod.Post, _serverUrl + request.Url);
        HttpContent content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
        
        requestMessage.Content = content;
        var response = await client.SendAsync(requestMessage);
        var responseString = await response.Content.ReadAsStringAsync();
        
        var responseData = JsonSerializer.Deserialize<TResponse>(responseString);
        return responseData;
    }
}