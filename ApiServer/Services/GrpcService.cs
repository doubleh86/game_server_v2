using System.Collections.Concurrent;
using Grpc.Core;
using Grpc.Net.Client;
using ServerFramework.CommonUtils.Helper;

namespace ApiServer.Services;

public class GrpcService(LoggerService loggerService)
{
    private GrpcChannel _channel;
    private ScheduleGRpcService.ScheduleGRpcServiceClient _grpcClient;
    private CancellationTokenSource _cancellationTokenSource;

    public void Initialize(string grpcServerAddress)
    {
        _channel = GrpcChannel.ForAddress(grpcServerAddress);
        _grpcClient = new ScheduleGRpcService.ScheduleGRpcServiceClient(_channel);
        
        _cancellationTokenSource = new CancellationTokenSource();
        Task.Run(() => SubscribeToEvents(_cancellationTokenSource.Token));
        
    }

    private async Task SubscribeToEvents(CancellationToken token)
    {
        try
        {
            var request = new SubscribeRequest { ServerName = "ApiServer" };
            using var call = _grpcClient.Subscribe(request, cancellationToken: token);

            await foreach (var response in call.ResponseStream.ReadAllAsync(token))
            {
                loggerService.Information($"Received gRPC Event: {response.EventName}, Payload: {response.Payload}");
                // TODO: Handle the event (e.g., update local cache, notify clients)
            }
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.Cancelled)
        {
            loggerService.Information("gRPC subscription cancelled.");
        }
        catch (Exception ex)
        {
            loggerService.Warning("grpc exception", ex);
            // Ideally add retry logic here
        }
    }
}