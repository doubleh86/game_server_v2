namespace ServerFramework.RedisService.Models;

public interface IRedisServiceFactory
{
    RedisServiceBaseAzure CreateRedisService(string key, RedisConnectionInfo connectionInfo);
}