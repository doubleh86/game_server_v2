using StackExchange.Redis;

namespace ServerFramework.RedisService;

public interface IRedisSubscriber
{
    ISubscriber Subscriber { get; set; }
    bool StartSubscribe();

    void OnSubscribe(RedisChannel channel, RedisValue message);
    void _RegisterHandler();
}