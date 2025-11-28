using Grpc.Core;
using Grpc.Net.Client;
using ServerFramework.CommonUtils.Helper;
using ServerFramework.GrpcServices;

namespace ApiServer.Services;

public class GrpcService(LoggerService loggerService, string serverName, string serverAddress) 
    : GrpcServiceBase<ScheduleGRpcService.ScheduleGRpcServiceClient>(loggerService, serverName, serverAddress)
{
    
    protected override ScheduleGRpcService.ScheduleGRpcServiceClient _CreateClient(GrpcChannel channel)
    {
        return new ScheduleGRpcService.ScheduleGRpcServiceClient(channel);
    }

    protected override async Task SubscribeToEvents(CancellationToken token)
    {
        try
        {
            var request = new SubscribeRequest { ServerName = "ApiServer" };
            using var call = GrpcClient.Subscribe(request, cancellationToken: token);
        
            await foreach (var response in call.ResponseStream.ReadAllAsync(token))
            {
                _loggerService.Information($"Received gRPC Event: {response.EventName}, Payload: {response.Payload}");
                // TODO: Handle the event (e.g., update local cache, notify clients)
            }
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.Cancelled)
        {
            _loggerService.Information("gRPC subscription cancelled.");
        }
        catch (Exception ex)
        {
            _loggerService.Warning("grpc exception", ex);
            // Ideally add retry logic here
        }
    }
}