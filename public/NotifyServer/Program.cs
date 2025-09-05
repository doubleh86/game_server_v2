// Start Console Program

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NotifyServer.Helpers;
using NotifyServer.Models.NetworkModels;
using NotifyServer.NetworkCommand;
using NotifyServer.Services.NetworkService;
using NotifyServer.Services.ServerService;
using Serilog;
using ServerFramework.CommonUtils.Helper;
using ServerFramework.RedisService;
using SuperSocket.Command;
using SuperSocket.Server;
using SuperSocket.Server.Host;

var host = CreateHostBuilder().Build();

try
{
    await host.RunAsync();
}
catch (Exception e)
{
    Log.Error(e.ToString());
}
finally
{
    Log.CloseAndFlush();
}

return;

IHostBuilder CreateHostBuilder()
{
    
    var builder = SuperSocketHostBuilder.Create<NetworkPackage, NotifyPackagePipeLineFilter>();
    
    builder.UseSession<UserSessionInfo>();
    builder.UseHostedService<NotifyServerService>();
    builder.UseCommand(InitializeCommand);
    
    builder.ConfigureServices((ctx, services) =>
    {
        services.AddSingleton<LoggerService>();
        services.AddSingleton<UserService>();
        services.AddSingleton<RedisServiceManager>();
        services.AddSingleton<ConfigurationHelper>();
    });
    
    builder.ConfigureLogging((ctx, logging) =>
    {
        logging.AddConsole();
    });
    
    return builder;
}

void InitializeCommand(CommandOptions options)
{
    options.AddCommand<Ping>();
    options.AddCommand<Test>();
} 