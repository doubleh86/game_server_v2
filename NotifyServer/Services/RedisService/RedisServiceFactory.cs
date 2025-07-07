using ServerFramework.RedisService;
using ServerFramework.RedisService.Models;

namespace NotifyServer.Services.RedisService;

public class RedisServiceFactory : IRedisServiceFactory
{
    public RedisServiceBaseAzure CreateRedisService(string key, RedisConnectionInfo connectionInfo)
    {
        return key switch
        {
            nameof(SessionRedisService) => new SessionRedisService(connectionInfo),
            _ => null
        };
    }
}