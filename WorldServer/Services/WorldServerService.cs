using Microsoft.Extensions.Options;
using ServerFramework.CommonUtils.DateTimeHelper;
using ServerFramework.CommonUtils.Helper;
using SuperSocket.Server;
using SuperSocket.Server.Abstractions;
using WorldServer.GameService;
using WorldServer.Network;

namespace WorldServer.Services;

public class WorldServerService : SuperSocketService<NetworkPackage>
{
    private readonly UserService _userService;
    private readonly WorldService _worldService;
    private readonly LoggerService _loggerService;
    
    private readonly ConfigurationHelper _configurationHelper;
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
    
    protected override async ValueTask OnStartedAsync()
    {
        var configFiles = new List<string> { "appsettings.json", "Settings/redisSettings.json" };
        _configurationHelper.Initialize(configFiles);
        _loggerService.CreateLogger(_configurationHelper.Configuration);
        
        var serviceTimeZone = _configurationHelper.GetValue("ServiceTimeZone", "UTC");
        TimeZoneHelper.Initialize(serviceTimeZone);
        
        _worldService.SetServerService(this);
        _worldService.StartGlobalTicker();
        
        await base.OnStartedAsync();
    }

    protected override async ValueTask OnStopAsync()
    {
        await base.OnStopAsync();
    }
    
}