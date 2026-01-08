using DbContext.GameDbContext;
using Microsoft.Extensions.Options;
using ServerFramework.CommonUtils.DateTimeHelper;
using ServerFramework.CommonUtils.Helper;
using ServerFramework.SqlServerServices.Models;
using SuperSocket.Server;
using SuperSocket.Server.Abstractions;
using WorldServer.Network;

namespace WorldServer.Services;

public class WorldServerService : SuperSocketService<NetworkPackage>
{
    private readonly UserService _userService;
    private readonly WorldService _worldService;
    private readonly LoggerService _loggerService;
    
    private readonly ConfigurationHelper _configurationHelper;
    private Dictionary<string, SqlServerDbInfo> _sqlServerDbInfoList = new();

    public UserService GetUserService() => _userService;
    public WorldService GetWorldService() => _worldService;
    public LoggerService GetLoggerService() => _loggerService;
    public ConfigurationHelper GetConfigHelper() => _configurationHelper;

    public WorldServerService(IServiceProvider serviceProvider, IOptions<ServerOptions> serverOptions) : base(serviceProvider, serverOptions)
    {
        _userService = serviceProvider.GetService(typeof(UserService)) as UserService;
        
        _worldService = serviceProvider.GetService(typeof(WorldService)) as WorldService;
        _loggerService = serviceProvider.GetService(typeof(LoggerService)) as LoggerService;
        _configurationHelper = serviceProvider.GetService(typeof(ConfigurationHelper)) as ConfigurationHelper;
    }
    
    private void _InitializeSqlServerDbInfo()
    {
        var sqlSettings = _configurationHelper.GetSection<SqlServerDbSettings>(nameof(SqlServerDbSettings));
        _sqlServerDbInfoList = sqlSettings.ConnectionInfos;
        
        foreach (var (key, value) in sqlSettings.ConnectionInfos)
        {
            switch (key)
            {
                case "GameDbContext":
                    GameDbContextWrapper.SetDefaultServerInfo(value);
                    break;
            }
        }
    }
    
    protected override async ValueTask OnStartedAsync()
    {
        var configFiles = new List<string> { "appsettings.json", "Settings/redisSettings.json", "Settings/sqlSettings.json"};
        _configurationHelper.Initialize(configFiles);
        _InitializeSqlServerDbInfo();
        _loggerService.CreateLogger(_configurationHelper.Configuration);
        
        var serviceTimeZone = _configurationHelper.GetValue("ServiceTimeZone", "UTC");
        TimeZoneHelper.Initialize(serviceTimeZone);
        
        var minWorker = _configurationHelper.GetValue("MinWorkerThreads", 120);
        var minIOThread = _configurationHelper.GetValue("MinIOThreads", 120);
        
        ThreadPool.SetMinThreads(Math.Max(minWorker, Environment.ProcessorCount * 2), minIOThread);
        
        _worldService.Initialize(this);
        _worldService.StartGlobalTicker();
        
        await base.OnStartedAsync();
    }

    protected override async ValueTask OnStopAsync()
    {
        await base.OnStopAsync();
    }
    
}