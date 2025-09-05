using System.Collections.Generic;
using AdminWeb.Services.Models;
using DbContext.AdminDbContext;
using DbContext.SharedContext;
using ServerFramework.CommonUtils.Helper;
using ServerFramework.SqlServerServices.Models;

namespace AdminWeb.Services;

/// <summary>
/// Program.cs 파일에 다음 코드를 추가하여 AdminToolServerService를 DI 컨테이너에 등록합니다:
/// <code>
/// builder.Services.AddSingleton&lt;AdminToolServerService&gt;();
/// </code>
/// 이후 Razor 컴포넌트에서 다음과 같이 주입하여 사용할 수 있습니다:
/// <code>
/// @inject AdminToolServerService ServerService
/// </code>
/// 이와 같이 설정하면 Service를 간편하게 사용할 수 있습니다.
/// </summary>

public class AdminToolServerService
{
    private readonly ConfigurationHelper _customConfiguration = new();
    private readonly LoggerService _loggerService = new();
    private ServerExtraOption _serverExtraOption;
    
    private Dictionary<string, SqlServerDbInfo> _sqlServerDbInfoList = new(); 
    public LoggerService LoggerService => _loggerService;
    public ServerExtraOption ServerExtraOption => _serverExtraOption;

    public void Initialize()
    {
        var configFiles = new List<string> {"appsettings.json", "Settings/sqlSettings.json"};
        
        _customConfiguration.Initialize(configFiles);
        _serverExtraOption = _customConfiguration.GetSection<ServerExtraOption>(nameof(ServerExtraOption));
        _loggerService.CreateLogger(_customConfiguration.Configuration);
        
        _InitializeSqlServerDbInfo();
    }
    
    private void _InitializeSqlServerDbInfo()
    {
        var sqlSettings = _customConfiguration.GetSection<SqlServerDbSettings>(nameof(SqlServerDbSettings));
        _sqlServerDbInfoList = sqlSettings.ConnectionInfos;

        foreach (var (key, value) in _sqlServerDbInfoList)
        {
            switch (key)
            {
                case nameof(SharedDbContext):
                    SharedDbContext.SetDefaultServerInfo(value);
                    break;
                case nameof(AdminDbContext):
                    AdminDbContext.SetDefaultServerInfo(value);
                    break;
            }
        }
    }
    
    public SqlServerDbInfo GetSqlServerDbInfo(string connectionString)
    {
        _sqlServerDbInfoList.TryGetValue(connectionString, out var dbInfo);
        return dbInfo;
    }
    
}