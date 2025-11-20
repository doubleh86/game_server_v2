using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Scheduler.Main;
using Scheduler.Services.gRPCService;

var builder = WebApplication.CreateBuilder(args);
var main = new ScheduleMain();
main.Initialize();

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(main.GetGrpcPort(), listenOptions =>
    {
        listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http2;
    });
});

builder.Services.AddGrpc();

var app = builder.Build();
app.MapGrpcService<ScheduleGrpcServiceImpl>();

main.Start();

await app.RunAsync();



