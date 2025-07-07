using Microsoft.Extensions.Configuration;
using RedLockNet;
using RedLockNet.SERedis;
using RedLockNet.SERedis.Configuration;
using ServerFramework.RedisService.Models;
using StackExchange.Redis;

namespace ServerFramework.RedisService;

public class RedLockManager: IDisposable, IAsyncDisposable
{
    private const string _RedLockPrefix = "RLOCK";
    
    private static TimeSpan _expiry = TimeSpan.FromSeconds(60);
    private static TimeSpan _waitTime = TimeSpan.FromSeconds(3);
    private static TimeSpan _retryTime = TimeSpan.FromSeconds(3);
    
    private readonly List<RedLockMultiplexer> _redLockMultiplexers = [];
    private RedLockFactory _redLockFactory;

    public RedLockFactory GetRedLockFactory() => _redLockFactory;
    
    private List<RedLockConnections> _redLockConnections = [];
    
    public bool InitializeRedLock(int expiryTime, int waitTime, int retryTime, IConfiguration configuration)
    {
        _redLockConnections = configuration.GetSection(RedLockConnections.SectionName).Get<List<RedLockConnections>>();
        if (_redLockConnections == null || _redLockConnections.Count == 0)
            return false;
        
        foreach (var connectionInfo in _redLockConnections)
        {
            var connectString = $"{connectionInfo.Host}:{connectionInfo.Port}, password={connectionInfo.Password}, abortConnect=true, ConnectTimeout=2000, ConnectRetry=3";
            _redLockMultiplexers.Add(ConnectionMultiplexer.Connect(connectString));
        }

        _redLockFactory = RedLockFactory.Create(_redLockMultiplexers);
        
        _expiry = TimeSpan.FromSeconds(expiryTime);
        _waitTime = TimeSpan.FromSeconds(waitTime);
        _retryTime = TimeSpan.FromSeconds(retryTime);

        return true;
    }

    public IRedLock CreateLock(string resource)
    {
        return _redLockFactory.CreateLock(resource, _expiry, _waitTime, _retryTime);
    }

    public async Task<IRedLock> CreateLockAsync(string resource)
    {
        var result = await _redLockFactory.CreateLockAsync(resource, _expiry, _waitTime, _retryTime);
        return result;
    }


    public static string GetRedLockResource(string accountId)
    {
        return $"{_RedLockPrefix}_{accountId}";
    }

    public void Dispose()
    {
        _redLockFactory.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        if (_redLockFactory != null)
            await Task.Run(() => _redLockFactory.Dispose());
    }
}