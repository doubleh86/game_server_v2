using System.Collections.Concurrent;
using Grpc.Core;
using Microsoft.AspNetCore.Http.Features;

namespace Scheduler.Services.gRPCService;

public class ScheduleGrpcServiceImpl : ScheduleGRpcService.ScheduleGRpcServiceBase
{
    private static readonly ConcurrentDictionary<string, IServerStreamWriter<ScheduleEvent>> _subScribers = new();
    public override async Task Subscribe(SubscribeRequest request, IServerStreamWriter<ScheduleEvent> responseStream, ServerCallContext context)
    {
        _subScribers[request.ServerName] = responseStream;
        
        try
        {
            await Task.Delay(-1, context.CancellationToken); // 연결 유지
        }
        catch
        {
            _subScribers.TryRemove(request.ServerName, out _);
        }
    }
}