using ServerFramework.RedisService;
using ServerFramework.RedisService.Models;

namespace NotifyServer.Services.RedisService;

public class SessionRedisService : RedisServiceBaseAzure
{
    public SessionRedisService(RedisConnectionInfo redisConnectionInfo) : base(redisConnectionInfo)
    {
    }
}