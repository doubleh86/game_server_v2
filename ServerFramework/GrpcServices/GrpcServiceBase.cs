using Grpc.Core;
using Grpc.Net.Client;
using ServerFramework.CommonUtils.Helper;

namespace ServerFramework.GrpcServices;

/// <summary>
/// 
/// </summary>
/// <param name="loggerService"></param>
/// <param name="serverName">자신의 서버이름</param>
/// <param name="serverAddress">구독할 서버 주소</param>
/// <typeparam name="TClientBase"></typeparam>
public abstract class GrpcServiceBase<TClientBase>(LoggerService loggerService, string serverName, string serverAddress)
    where TClientBase : ClientBase
{
    private GrpcChannel _channel;
    private CancellationTokenSource _cancellationTokenSource;
    private TClientBase _client;
    
    protected readonly LoggerService _loggerService = loggerService;
    protected abstract TClientBase _CreateClient(GrpcChannel channel);
    protected abstract Task SubscribeToEvents(CancellationToken token);
    
    public TClientBase GrpcClient => _client;
    public void Initialize()
    {
        _channel = GrpcChannel.ForAddress(serverAddress);
        _client = _CreateClient(_channel);
        
        _cancellationTokenSource = new CancellationTokenSource();
        Task.Run(() => SubscribeToEvents(_cancellationTokenSource.Token));
        
        _loggerService.Information($"Initializing {serverName} server at {serverAddress}");
    }
}