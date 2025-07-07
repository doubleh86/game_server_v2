using ServerFramework.CommonUtils.Helper;
using ServerFramework.RedisService.Models;

namespace ServerFramework.RedisService;

public class RedisServiceManager
{
    private RedisSettings _redisSettings;
    private IRedisServiceFactory _factory;
    private LoggerService _loggerService;
    
    private readonly Dictionary<string, List<RedisServiceBaseAzure>> _redisServices = new();
    public bool Initialize(RedisSettings redisSettings, IRedisServiceFactory factory, LoggerService loggerService)
    {
        if (redisSettings == null)
            return false;

        if (factory == null)
            return false;
        
        _loggerService = loggerService;
        
        _factory = factory;
        _redisSettings = redisSettings;
        foreach (var (key, list) in _redisSettings.ConnectionInfos)
        {
            var result = new List<RedisServiceBaseAzure>();
            foreach(var connectionInfo in list)
            {
                var serviceInfo = _factory.CreateRedisService(key, connectionInfo);
                if(serviceInfo == null)
                    continue;

                result.Add(serviceInfo);
                _loggerService?.Information($"Connected : [ServerKey : {key}][Host: {connectionInfo.Host}][Port: {connectionInfo.Port}][Channel: {connectionInfo.Channel}]");
            }
            
            if(result.Count < 1)
                continue;
            
            _redisServices.Add(key, result);
        }

        return true;
    }
    
    public TService GetRedisService<TService>(long accountId) where TService : RedisServiceBaseAzure
    {
        var serviceName = typeof(TService).Name;
        if (_redisServices.TryGetValue(serviceName, out var serviceList) == false)
            return null;

        var serviceIndex = CommonHelper.GetDbId(accountId.ToString(), serviceList.Count);
        return serviceList[serviceIndex] as TService;
    }

    public TService GetRedisService<TService>() where TService : RedisServiceBaseAzure
    {
        var serviceName = typeof(TService).Name;
        if (_redisServices.TryGetValue(serviceName, out var serviceList) == false)
            return null;

        return CommonHelper.Shuffle(serviceList).FirstOrDefault() as TService;
    }
    
    /// <summary>
    /// Subscribe 는 무조건 첫번째
    /// </summary>
    /// <typeparam name="TService"></typeparam>
    /// <returns></returns>
    public TService GetRedisSubscribe<TService>() where TService : RedisServiceBaseAzure
    {
        var serviceName = typeof(TService).Name;
        if (_redisServices.TryGetValue(serviceName, out var serviceList) == false)
            return null;

        return serviceList.FirstOrDefault() as TService;
    }

    /// <summary>
    /// Subscribe 는 무조건 첫번째
    /// </summary>
    public void StartSubscribe()
    {
        foreach (var service in _redisServices)
        {
            if (service.Value.Count < 1)
                continue;

            var subscribeService = service.Value.FirstOrDefault() as IRedisSubscriber;
            subscribeService?.StartSubscribe();
        }
    }

}