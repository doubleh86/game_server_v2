using NotifyServer.Services.RedisService;
using ServerFramework.RedisService;
using ServerFramework.RedisService.Models;

namespace ApiServer.Services;

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