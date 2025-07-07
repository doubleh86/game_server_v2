// using ServerFramework.CommonUtils.Helper;
// using ServerFramework.RedisService.Models;
// using StackExchange.Redis;
//
// // ReSharper disable InconsistentNaming
//
// namespace ServerFramework.RedisService;
//
// /// <summary>
// /// 해당 클래스를 생성하면 싱글턴으로 만들어서 사용.
// /// -- RedisServiceBaseAzure 사용 --
// /// </summary>
// public abstract class RedisServiceBase
// {
//     private readonly string _connectionName;
//     private readonly int _dbId = -1;
//     protected IDatabase _Database => _multiplexer?.Value.GetDatabase(_dbId);
//     protected readonly RedisConnectionInfo _redisConnectionInfo;
//     private Lazy<ConnectionMultiplexer> _multiplexer;
//
//     // Real Time 서버에서 Protected 로 사용 중.
//     protected ConnectionMultiplexer _Connection => _multiplexer?.Value;
//
//     // Real Time 서버에서 사용 중.
//     public bool IsConnected => _Connection != null;
//     protected LoggerService _loggerService;
//
//     protected RedisServiceBase(RedisSettings settings, LoggerService loggerService)
//     {
//         _connectionName = GetType().Name;
//         _loggerService = loggerService;
//         settings.ConnectionInfos.TryGetValue(_connectionName, out _redisConnectionInfo);
//         if (_redisConnectionInfo == null)
//             return;
//
//         _dbId = RedisServiceHelper.GetDatabaseId(_connectionName);
//         _Initialized(_redisConnectionInfo, true);
//     }
//     
//     protected RedisServiceBase(RedisConnectionInfo redisConnectionInfo)
//     {
//         _connectionName = GetType().Name;
//         _redisConnectionInfo = redisConnectionInfo;
//         _dbId = RedisServiceHelper.GetDatabaseId(_connectionName);
//
//         _Initialized(_redisConnectionInfo, true);
//     }
//
//     private void _Initialized(RedisConnectionInfo redisConnectionInfo, bool isConstructCall = false)
//     {
//         if (_Connection != null)
//         {
//             return;
//         }
//
//         try
//         {
//             var connectionString = $"{redisConnectionInfo.Host}:{redisConnectionInfo.Port}, password={redisConnectionInfo.Password},abortConnect=false, ConnectTimeout=2000, ConnectRetry=3";
//             _multiplexer = new Lazy<ConnectionMultiplexer>(() => ConnectionMultiplexer.Connect(connectionString), LazyThreadSafetyMode.PublicationOnly);
//             if (isConstructCall == false)
//                 return;
//
//             _RegisterRedisCallbackMethod();
//         }
//         catch (Exception e)
//         {
//             _loggerService?.Warning($"Redis Connection Failed [Name: {_connectionName}][Host:{redisConnectionInfo.Host}][Port: {redisConnectionInfo.Port}]", e);
//             throw;
//         }
//     }
//
//     public async Task<IEnumerable<HashEntry>> HGetAllAsync(string key)
//     {
//         if (_CheckRedisConnection() == false)
//             return null;
//
//         if (_Database == null)
//             return null;
//
//         return await _Database.HashGetAllAsync(key);
//     }
//
//     public IEnumerable<HashEntry> HGetAll(string key)
//     {
//         if (_CheckRedisConnection() == false)
//             return null;
//
//         return _Database?.HashGetAll(key);
//     }
//
//     protected bool KeyDelete(string key)
//     {
//         if (_CheckRedisConnection() == false)
//             return false;
//
//         return _Database?.KeyDelete(key) ?? false;
//     }
//
//     protected bool KeyExists(string key)
//     {
//         if (_CheckRedisConnection() == false)
//             return false;
//
//         return _Database?.KeyExists(key) ?? false;
//     }
//
//
//     public IEnumerable<HashEntry> HScan(string key)
//     {
//         if (_CheckRedisConnection() == false)
//             return null;
//
//         return _Database?.HashScan(key);
//     }
//
//     private bool _CheckRedisConnection()
//     {
//         return true;
//     }
//
//     public async Task<bool> HSetAsync(IRedisHash hashData)
//     {
//         try
//         {
//             if (_CheckRedisConnection() == false)
//                 return false;
//
//             if (_Database == null)
//                 return false;
//
//             await _Database.HashSetAsync(hashData.GetKey(), hashData.GetRedisHashData());
//             return true;
//         }
//         catch (Exception e)
//         {
//             _loggerService?.Error("Redis Failed", e);
//             return false;
//         }
//     }
//
//     protected async Task<bool> HSetAsync(string key, HashEntry[] hashEntries)
//     {
//         try
//         {
//             if (_CheckRedisConnection() == false)
//                 return false;
//
//             if (_Database == null)
//                 return false;
//
//             await _Database.HashSetAsync(key, hashEntries);
//             return true;
//         }
//         catch (Exception e)
//         {
//             _loggerService?.Error("Redis Failed", e);
//             return false;
//         }
//     }
//
//     public bool HSet(IRedisHash hashData)
//     {
//         try
//         {
//             if (_CheckRedisConnection() == false)
//                 return false;
//
//             _Database?.HashSet(hashData.GetKey(), hashData.GetRedisHashData());
//             return true;
//         }
//         catch (Exception e)
//         {
//             _loggerService?.Error("Redis Failed", e);
//             return false;
//         }
//     }
//
//     public bool HSet(string key, HashEntry[] hashEntries)
//     {
//         try
//         {
//             if (_CheckRedisConnection() == false)
//                 return false;
//
//             if (_Database == null)
//                 return false;
//
//             _Database.HashSet(key, hashEntries);
//             return true;
//         }
//         catch (Exception e)
//         {
//             _loggerService?.Error("Redis Failed", e);
//             return false;
//         }
//     }
//
//
//
//     protected RedisValue? _StringGet(string key)
//     {
//         if (_CheckRedisConnection() == false)
//             return RedisValue.Null;
//
//         return _Database?.StringGet(key);
//     } 
//     protected bool _StringSet(string key, RedisValue data)
//     {
//         try
//         {
//             if (_CheckRedisConnection() == false)
//                 return false;
//
//             _Database?.StringSet(key, data);
//             return true;
//         }
//         catch (Exception e)
//         {
//             _loggerService?.Error("Redis Failed", e);
//             return false;
//         }
//     }
//     
//     public bool SetHashData(string redisKey, string fieldName, RedisValue value)
//     {
//         try
//         {
//             if (_CheckRedisConnection() == false)
//                 return false;
//
//             _Database?.HashSet(redisKey, fieldName, value);
//             return true;
//         }
//         catch (Exception e)
//         {
//             _loggerService?.Error("Redis Failed", e);
//             return false;
//         }
//     }
//
//
//     protected void Publish(string publishData)
//     {
//         if (string.IsNullOrWhiteSpace(_redisConnectionInfo.Channel) == true)
//             return;
//
//         if (_CheckRedisConnection() == false)
//             return;
//
//         try
//         {
//             var publisher = _Connection.GetSubscriber();
//             var redisChannel = new RedisChannel(_redisConnectionInfo.Channel, RedisChannel.PatternMode.Auto);
//             publisher.Publish(redisChannel, publishData);
//         }
//         catch (Exception e)
//         {
//             _loggerService?.Warning("Redis Publish Error", e);
//         }
//     }
//
//     protected async Task PublishAsync(string publishData)
//     {
//         if (string.IsNullOrWhiteSpace(_redisConnectionInfo.Channel) == true)
//             return;
//
//         if (_Database == null)
//             return;
//
//         try
//         {
//             var publisher = _Connection.GetSubscriber();
//             var redisChannel = new RedisChannel(_redisConnectionInfo.Channel, RedisChannel.PatternMode.Auto);
//
//             await publisher.PublishAsync(redisChannel, publishData);
//         }
//         catch (Exception e)
//         {
//             _loggerService?.Warning("Redis Publish Error", e);
//         }
//     }
//         
//     protected IEnumerable<RedisKey> Keys(RedisValue pattern)
//     {
//         return _Database?.Multiplexer.GetServer(_redisConnectionInfo.Host, _redisConnectionInfo.Port).Keys(pattern: pattern);
//     }
//
//     private void _RegisterRedisCallbackMethod()
//     {
//         _Connection.ConnectionFailed += OnConnectionFailed;
//         _Connection.ConnectionRestored += OnConnectionRestored;
//         _Connection.InternalError += OnRedisInternalError;
//         _Connection.ErrorMessage += OnErrorMessage;
//     }
//
//     private void OnRedisInternalError(object sender, InternalErrorEventArgs e)
//     {
//         _loggerService?.Error($"[ConnName : {_connectionName}][Channel : {_redisConnectionInfo.Channel}][InternalErrorEventArgs : {e}] Redis Internal Error");
//     }
//
//     private void OnConnectionRestored(object sender, ConnectionFailedEventArgs e)
//     {
//         _loggerService?.Error($"[ConnName : {_connectionName}][Channel : {_redisConnectionInfo.Channel}][ConnectionFailedEventArgs : {e}] Redis Connected Restored");
//     }
//
//     private void OnConnectionFailed(object sender, ConnectionFailedEventArgs e)
//     {
//         _loggerService?.Error($"[ConnName : {_connectionName}][Channel : {_redisConnectionInfo.Channel}][ConnectionFailedEventArgs : {e}] Redis Connected Fail");
//     }
//
//     private void OnErrorMessage(object sender, RedisErrorEventArgs e)
//     {
//         _loggerService?.Error($"[ConnName : {_connectionName}][Channel : {_redisConnectionInfo.Channel}][RedisErrorEventArgs : {e}] Redis Error ");
//     }
//
//
//     protected void RunLuaScript(string script)
//     {
//         var prepared = LuaScript.Prepare(script);
//         _Database.ScriptEvaluate(prepared);
//     }
//
//     protected RedisValue? HGet(string hashKey, string hashField)
//     {
//         try
//         {
//             if (_CheckRedisConnection() == false)
//                 return null;
//
//             return _Database?.HashGet(new RedisKey(hashKey), new RedisValue(hashField));
//         }
//         catch (Exception e)
//         {
//             _loggerService?.Error("Redis Failed", e);
//             return null;
//         }
//     }
//
// }