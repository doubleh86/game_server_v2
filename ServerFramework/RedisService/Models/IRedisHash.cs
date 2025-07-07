using StackExchange.Redis;

namespace ServerFramework.RedisService.Models;

public interface IRedisHash
{
    HashEntry[] GetRedisHashData();
    string GetKey();
}