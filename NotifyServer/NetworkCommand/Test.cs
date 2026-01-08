using NetworkProtocols;
using NetworkProtocols.Socket;
using NetworkProtocols.Socket.NotifyServerProtocols;
using NetworkProtocols.Socket.NotifyServerProtocols.TwoWayModels;
using NotifyServer.Helpers;
using NotifyServer.Models.NetworkModels;
using ServerFramework.CommonUtils.Helper;
using SuperSocket.Command;
using SuperSocket.Server.Abstractions.Session;

namespace NotifyServer.NetworkCommand;

[Command(Key = NotifyServerKeys.RequestTest)]
public class Test : IAsyncCommand<NetworkPackage>
{
    private readonly LoggerService _loggerService;
    public Test(LoggerService loggerService)
    {
        _loggerService = loggerService;
    }
    
    public async ValueTask ExecuteAsync(IAppSession session, NetworkPackage package, CancellationToken cancellationToken)
    {
        var receivedPackage = MemoryPackHelper.Deserialize<TestCommandRequest>(package.Body);
        if(receivedPackage == null)
            return;

        var returnCommand = new TestCommandResponse
        {
            TestText = receivedPackage.TestText
        };
        
        _loggerService.Information($"Test Received {receivedPackage.TestText}");
        var sendPackage = NetworkHelper.CreateSendPacket((int)NotifyServerKeys.ResponseTest, returnCommand);
        
        await session.SendAsync(sendPackage.GetSendBuffer(), cancellationToken);
    }
}