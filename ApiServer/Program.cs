using ApiServer.Services;
using DataTableLoader.Utils;
using DataTableLoader.Utils.Helper;
using DbContext.SharedContext.MySqlContext;
using DbContext.SharedContext.SqlServerContext;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Serilog;
using ServerFramework.CommonUtils.DateTimeHelper;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.WebHost.ConfigureKestrel((_, options) =>
{
    options.Limits.MaxRequestBodySize = 1024 * 1024 * 100;
    options.Limits.MinRequestBodyDataRate = new MinDataRate(bytesPerSecond: 100, gracePeriod: TimeSpan.FromSeconds(10));
    options.Limits.KeepAliveTimeout = TimeSpan.FromSeconds(30);
});

#region Add Services

builder.Services.AddSingleton<ApiServerService>();
builder.Services.AddSingleton<EventService>();

#endregion


var app = builder.Build();

app.UseAuthorization();
app.MapControllers();

if (InitializeServices(app.Services) == false)
{
    Console.WriteLine("Initialize service failed");
    return;
}

if (await InitializeEventService(app.Services) == false)
{
    Console.WriteLine("Initialize event service failed");
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
        $"= [Start Server Time : {TimeZoneHelper.ServerTimeNow}]\n" +
        $"=================================================================";
    
    serverService.LoggerService.Information("Service initialized Complete");
    serverService.LoggerService.Information(serverStartInfoLog);

    var sqlInfo = serverService.GetSqlServerDbInfo(nameof(DataTableDbService));
    DataHelper.Initialize(sqlInfo, serverService.LoggerService);
    DataHelper.ReloadTableData();

    return true;
}

async Task<bool> InitializeEventService(IServiceProvider provider)
{
    var serverService = provider.GetService<EventService>();
    if (serverService == null)
    {
        return false;
    }
    
    using var mysqlDbContext = MySqlSharedDbContext.Create();
    var eventList = await mysqlDbContext.GetEventInfoListAsync();

    // using var dbContext = SharedDbContext.Create();
    // var eventList = await dbContext.GetEventInfoListAsync();
    serverService.Initialize(eventList ?? []);

    return true;
}
