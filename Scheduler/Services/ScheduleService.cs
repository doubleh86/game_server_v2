using DbContext.SharedContext;
using ServerFramework.CommonUtils.Helper;
using ServerFramework.SqlServerServices.Models;
using SharedDbContext = DbContext.SharedContext.SqlServerContext.SharedDbContext;

namespace Scheduler.Services;

public class ScheduleService : IDisposable
{
    private readonly ConfigurationHelper _customConfiguration = new();
    private readonly LoggerService _loggerService = new();
    
    private Dictionary<string, SqlServerDbInfo> _sqlServerDbInfoList = new();
    public ConfigurationHelper CustomConfiguration => _customConfiguration;
    public LoggerService LoggerService => _loggerService;

    public void Initialize()
    {
        var configFiles = new List<string> { "appsettings.json", "Settings/redisSettings.json", "Settings/sqlSettings.json"};

        _customConfiguration.Initialize(configFiles);
        _loggerService.CreateLogger(_customConfiguration.Configuration);

        _InitializeSqlServerDbInfo();
    }
    
    private void _InitializeSqlServerDbInfo()
    {
        var sqlSettings = _customConfiguration.GetSection<SqlServerDbSettings>(nameof(SqlServerDbSettings));
        _sqlServerDbInfoList = sqlSettings.ConnectionInfos;
        foreach (var (key, value) in sqlSettings.ConnectionInfos)
        {
            switch (key)
            {
                case nameof(SharedDbContext):
                    SharedDbContext.SetDefaultServerInfo(value);
                    break;
            }
        }
    }
    
    public SqlServerDbInfo GetSqlServerDbInfo(string connectionString)
    {
        _sqlServerDbInfoList.TryGetValue(connectionString, out var dbInfo);
        return dbInfo;
    }
    
    public void Dispose()
    {
        // TODO release managed resources here
    }
}