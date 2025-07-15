using ApiServer.Services;
using DataTableLoader.Utils;
using DataTableLoader.Utils.Helper;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Serilog;
using ServerFramework.CommonUtils.DateTimeHelper;
using ServerFramework.CommonUtils.Helper;
using ServerFramework.SqlServerServices.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.WebHost.ConfigureKestrel((_, options) =>
{
    options.Limits.MaxRequestBodySize = 1024 * 1024 * 100;
    options.Limits.MinRequestBodyDataRate = new MinDataRate(bytesPerSecond: 100, gracePeriod: TimeSpan.FromSeconds(10));
    options.Limits.KeepAliveTimeout = TimeSpan.FromSeconds(30);
});

builder.Services.AddSingleton<ApiServerService>();

var app = builder.Build();

app.UseAuthorization();
app.MapControllers();

if (InitializeServices(app.Services) == false)
{
    Console.WriteLine("Initialize service failed");
    return;
}

app.Run();
Log.CloseAndFlush();

return;

bool InitializeServices(IServiceProvider provider)
{
    var serverService = provider.GetService<ApiServerService>();
    if (serverService == null)
        return false;
    
    serverService.Initialize();
    var serviceTimeZone = serverService.CustomConfiguration.GetValue("ServiceTimeZone", "UTC");
    TimeZoneHelper.Initialize(serviceTimeZone);
    
    var serverStartInfoLog = 
        $"\n" +
        $"=============Server initialized successfully=====================\n" +
        $"= [TimeZoneInfo : {serviceTimeZone}]\n" +
        $"= [Start Server Time : {TimeZoneHelper.UtcNow.ToServerTime()}]\n" +
        $"=================================================================";
    
    serverService.LoggerService.Information("Service initialized Complete");
    serverService.LoggerService.Information(serverStartInfoLog);

    var sqlInfo = serverService.GetSqlServerDbInfo(nameof(DataTableDbService));
    DataHelper.Initialize(sqlInfo, serverService.LoggerService);
    DataHelper.LoadAllTableData();

    return true;
}
