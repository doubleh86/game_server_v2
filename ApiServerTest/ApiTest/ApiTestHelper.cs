using System.Text;
using System.Text.Json;
using NetworkProtocols.WebApi;
using NetworkProtocols.WebApi.Commands;

namespace ApiServerTest.ApiTest;

public static class ApiTestHelper
{
    public static async Task<TResponse> SendPacket<T, TResponse>(T request, string serverUrl) where T : RequestBase where TResponse : ResponseBase, new() 
    {
        using var client = new HttpClient();
        using var requestMessage = new HttpRequestMessage(HttpMethod.Post, serverUrl + request.Url);
        HttpContent content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
        
        requestMessage.Content = content;
        var response = await client.SendAsync(requestMessage);
        if (response.IsSuccessStatusCode == true)
        {
            var responseString = await response.Content.ReadAsStringAsync();
        
            var responseData = JsonSerializer.Deserialize<TResponse>(responseString);
            return responseData;    
        }

        var errorResponse = new TResponse
        {
            DebugMessage = $"[{response.StatusCode}] Error",
            ResultCode = (int)GameResultCode.SystemError,
        };
        
        return errorResponse; 
    }
}