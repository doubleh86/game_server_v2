using DbContext.SharedContext;
using ServerFramework.CommonUtils.Helper;
using ServerFramework.RedisService;
using ServerFramework.RedisService.Models;
using ServerFramework.SqlServerServices.Models;

namespace ApiServer.Services;

public partial class ApiServerService : IDisposable, IAsyncDisposable
{
    
    private readonly ConfigurationHelper _customConfiguration = new();
    private readonly LoggerService _loggerService = new();
    private readonly RedisServiceManager _redisServiceManager = new();
    private readonly RedLockManager _lockManager = new();
    private Dictionary<string, SqlServerDbInfo> _sqlServerDbInfoList = new();
    public LoggerService LoggerService => _loggerService;
    public ConfigurationHelper CustomConfiguration => _customConfiguration;
    public RedLockManager LockManager => _lockManager;

    

    public void Initialize()
    {
        var configFiles = new List<string> { "appsettings.json", "Settings/redisSettings.json", "Settings/sqlSettings.json"};

        _customConfiguration.Initialize(configFiles);
        _loggerService.CreateLogger(_customConfiguration.Configuration);
        
        _InitializeRedisService();
        _InitializeSqlServerDbInfo();
        _InitializeRedLockManager();
    }

    private void _InitializeRedLockManager()
    {
        var lockExpiryTime = _customConfiguration.GetValue("LockExpiryTime", 30);
        var lockWaitTime = _customConfiguration.GetValue("LockWaitTime", 10);
        var lockRetryTime = _customConfiguration.GetValue("LockRetryTime", 1);
    
        _lockManager.InitializeRedLock(lockExpiryTime, lockWaitTime, lockRetryTime, _customConfiguration.Configuration);
    }

    private void _InitializeRedisService()
    {
        var redisSettings = _customConfiguration.GetSection<RedisSettings>(nameof(RedisSettings));
        _redisServiceManager.Initialize(redisSettings, new RedisServiceFactory(), _loggerService);
        _redisServiceManager.StartSubscribe();
    }

    private void _InitializeSqlServerDbInfo()
    {
        var sqlSettings = _customConfiguration.GetSection<SqlServerDbSettings>(nameof(SqlServerDbSettings));
        _sqlServerDbInfoList = sqlSettings.ConnectionInfos;
        
        foreach (var (key, value) in sqlSettings.ConnectionInfos)
        {
            switch (key)
            {
                case "SharedDbContext":
                    SharedDbContextWrapper.SetDefaultServerInfo(value);
                    break;
            }
        }
    }

    public T GetRedisService<T>(long? accountId = null) where T : RedisServiceBaseAzure
    {
        if(accountId == null)
            return _redisServiceManager.GetRedisService<T>();
        
        return _redisServiceManager.GetRedisService<T>(accountId.Value);
    }
    
    public SqlServerDbInfo GetSqlServerDbInfo(string connectionString)
    {
        _sqlServerDbInfoList.TryGetValue(connectionString, out var dbInfo);
        return dbInfo;
    }

    public void Dispose()
    {
        _lockManager?.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        if (_lockManager != null) 
            await _lockManager.DisposeAsync();
    }
    
}