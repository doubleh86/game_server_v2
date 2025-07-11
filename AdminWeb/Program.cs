using AdminWeb.Components;
using AdminWeb.Services;
using AdminWeb.Services.GameUserService;
using BlazorDownloadFile;
using Blazored.SessionStorage;
using DataTableLoader.Utils;
using DataTableLoader.Utils.Helper;
using ServerFramework.CommonUtils.Helper;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
       .AddInteractiveServerComponents();

AddCustomServices(builder.Services);


var app = builder.Build();
await InitializeService(app.Services);

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
   .AddInteractiveServerRenderMode();

app.Run();

void AddCustomServices(IServiceCollection services)
{
    services.AddBlazorBootstrap();
    services.AddBlazoredSessionStorage();
    services.AddBlazorDownloadFile();
    
    services.AddSingleton<AdminToolServerService>();
    services.AddSingleton<CachedService>();
    services.AddSingleton<EventInfoService>();
    services.AddScoped<AdminUserService>();
    services.AddScoped<GameUserInfoService>();
    
    services.AddHttpContextAccessor();
    services.AddScoped<LanguageService>();
    
    TimeZoneHelper.Initialize("Asia/Seoul");
}

async Task InitializeService(IServiceProvider provider)
{
    var serverService = provider.GetService<AdminToolServerService>();
    if (serverService == null)
        return;
    
    serverService.Initialize();
    serverService.LoggerService.Information("Service initialized Complete");

    var sqlInfo = serverService.GetSqlServerDbInfo(nameof(DataTableDbService));
    DataHelper.Initialize(sqlInfo, serverService.LoggerService);
    DataHelper.LoadAllTableData();
    
    var cacheService = provider.GetService<CachedService>();
    if (cacheService == null)
    {
        serverService.LoggerService.Error("cacheService initialized Failed");
        return;
    }
    
    await cacheService.InitializeAsync();
    
    var eventService = provider.GetService<EventInfoService>();
    if (eventService == null)
    {
        serverService.LoggerService.Error("eventService initialized Failed");
        return;
    }
    
    await eventService.InitializeAsync();
}