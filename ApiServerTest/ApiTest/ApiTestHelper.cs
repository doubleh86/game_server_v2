using System.Text;
using System.Text.Json;
using NetworkProtocols.WebApi.Commands;

namespace ApiServerTest.ApiTest;

public static class ApiTestHelper
{
    public static async Task<TResponse> SendPacket<T, TResponse>(T request, string serverUrl) where T : RequestBase
    {
        using var client = new HttpClient();
        using var requestMessage = new HttpRequestMessage(HttpMethod.Post, serverUrl + request.Url);
        HttpContent content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
        
        requestMessage.Content = content;
        var response = await client.SendAsync(requestMessage);
        var responseString = await response.Content.ReadAsStringAsync();
        
        var responseData = JsonSerializer.Deserialize<TResponse>(responseString);
        return responseData;
    }
}