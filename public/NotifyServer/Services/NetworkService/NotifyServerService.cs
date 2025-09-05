using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using NotifyServer.Helpers;
using NotifyServer.Models.NetworkModels;
using NotifyServer.Services.RedisService;
using NotifyServer.Services.ServerService;
using ServerFramework.CommonUtils.Helper;
using ServerFramework.RedisService;
using ServerFramework.RedisService.Models;
using SuperSocket.Server;
using SuperSocket.Server.Abstractions;

namespace NotifyServer.Services.NetworkService;

public class NotifyServerService : SuperSocketService<NetworkPackage>
{
    private readonly UserService _userService;
    private readonly LoggerService _loggerService;
    private readonly RedisServiceManager _redisServiceManager;
    private readonly ConfigurationHelper _configurationHelper;
    public UserService GetUserService() => _userService;
    public LoggerService GetLoggerService() => _loggerService;
    public ConfigurationHelper GetConfigHelper() => _configurationHelper;
    
    public NotifyServerService(IServiceProvider serviceProvider, IOptions<ServerOptions> serverOptions) : base(serviceProvider, serverOptions)
    {
        _userService = serviceProvider.GetService(typeof(UserService)) as UserService;
        _loggerService = serviceProvider.GetService(typeof(LoggerService)) as LoggerService;
        _redisServiceManager = serviceProvider.GetService(typeof(RedisServiceManager)) as RedisServiceManager;
        _configurationHelper = serviceProvider.GetService(typeof(ConfigurationHelper)) as ConfigurationHelper;
    }

    protected override async ValueTask OnStartedAsync()
    {
        var configFiles = new List<string> { "appsettings.json", "Settings/redisSettings.json" };
        _configurationHelper.Initialize(configFiles);
        _loggerService.CreateLogger(_configurationHelper.Configuration);
        
        _Initialize();
        
        await base.OnStartedAsync();
    }

    protected override async ValueTask OnStopAsync()
    {
        await base.OnStopAsync();
    }

    private void _Initialize()
    {
        var redisSettings = _configurationHelper.GetSection<RedisSettings>(nameof(RedisSettings));
        _redisServiceManager.Initialize(redisSettings, new RedisServiceFactory(), _loggerService);
        _redisServiceManager.StartSubscribe();
    }
}