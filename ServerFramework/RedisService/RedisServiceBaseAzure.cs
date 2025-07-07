using ServerFramework.RedisService.Models;
using StackExchange.Redis;

// ReSharper disable InconsistentNaming
// ReSharper disable PrivateFieldCanBeConvertedToLocalVariable

namespace ServerFramework.RedisService;

/// <summary>
/// https://github.com/Azure-Samples/azure-cache-redis-samples/tree/main/quickstart/dotnet-core RedisConnection 으로 Redis 연결 관리
///  -> Redis 연결을 유지 및 Pool 관리
///  -> 해당 Redis의 경우 재사용을 알아서 해주기 때문에 사용 완료 후 Dispose() 또는 Close() 로 연결을 끊어줄 필요가 없다.
/// </summary>
public class RedisServiceBaseAzure : IDisposable
{
    private readonly string _connectionName;
    private readonly int _dbId;

    private readonly RedisConnectionInfo _redisConnectionInfo;
    private RedisConnection _redisConnection;

    public string GetConnectionInfo() => _redisConnection.GetConnectionString();

    protected RedisServiceBaseAzure(RedisConnectionInfo redisConnectionInfo)
    {
        _connectionName = GetType().Name;
        _redisConnectionInfo = redisConnectionInfo;
        _dbId = RedisServiceHelper.GetDatabaseId(_connectionName);

        _Initialized(_redisConnectionInfo);
    }

    private void _Initialized(RedisConnectionInfo redisConnectionInfo)
    {
        var connectionString = $"{redisConnectionInfo.Host}:{redisConnectionInfo.Port}, password={redisConnectionInfo.Password},abortConnect=false, ConnectTimeout=2000, ConnectRetry=3";
        _redisConnection = Task.Run(() => RedisConnection.InitializeAsync(connectionString)).Result;
    }

    public async Task<string> StringGetAsync(string key)
    {
        return await _redisConnection.BasicRetryAsync(async (db) => await db.Multiplexer.GetDatabase(_dbId).StringGetAsync(key));
    }

    public async Task<bool> StringSetAsync(string key, RedisValue data, int ttl = 0)
    {
        if (ttl < 1)
        {
            return await _redisConnection.BasicRetryAsync(async (db) => await db.Multiplexer.GetDatabase(_dbId).StringSetAsync(key, data));
        }

        return await _redisConnection.BasicRetryAsync(async (db) => await db.Multiplexer.GetDatabase(_dbId).StringSetAsync(key, data, expiry: TimeSpan.FromSeconds(ttl)));
    }

    protected async Task<bool> SubscribeAsync(Action<RedisChannel, RedisValue> handler)
    {
        return await _redisConnection.BasicRetryAsync(async (db) =>
        {
            var subscriber = db.Multiplexer.GetSubscriber();

            /*------------------------------------------------------------------------------------
            [smbaek / 2024.07.18]
            [Comment] : 형식 또는 멤버는 사용되지 않습니다. (단순 경고 제거)
            ------------------------------------------------------------------------------------*/
            #pragma warning disable CS0618
            await subscriber.SubscribeAsync(_redisConnectionInfo.Channel, handler);
            #pragma warning restore CS0618
            /*------------------------------------------------------------------------------------
            [smbaek] Comment end
            ------------------------------------------------------------------------------------*/
            return true;

        });
    }

    protected async Task PublishAsync(string publishData)
    {
        await _redisConnection.BasicRetryAsync(async (db) =>
        {
            var publisher = db.Multiplexer.GetSubscriber();
            var redisChannel = new RedisChannel(_redisConnectionInfo.Channel, RedisChannel.PatternMode.Auto);

            await publisher.PublishAsync(redisChannel, publishData);
            return true;
        });
    }

    protected async Task<bool> _KeyExpireAsync(string key, int expireSeconds)
    {
        return await _redisConnection.BasicRetryAsync(async (db) =>
        {
            await db.KeyExpireAsync(key, TimeSpan.FromSeconds(expireSeconds));
            return true;
        });
    }

    protected async Task<bool> _KeyExistAsync(string key)
    {
        return await _redisConnection.BasicRetryAsync(async (db) => await db.Multiplexer.GetDatabase(_dbId).KeyExistsAsync(key));
    }

    protected async Task<bool> _KeyDeleteAsync(string key)
    {
        return await _redisConnection.BasicRetryAsync(async (db) => await db.Multiplexer.GetDatabase(_dbId).KeyDeleteAsync(key));
    }

    public async Task RunLuaScript(string script)
    {
        await _redisConnection.BasicRetryAsync(async (db) =>
        {
            var prepared = LuaScript.Prepare(script);
            await db.Multiplexer.GetDatabase(_dbId).ScriptEvaluateAsync(prepared);
            return true;
        });
    }

    public void Dispose()
    {
        _redisConnection?.Dispose();
    }
}