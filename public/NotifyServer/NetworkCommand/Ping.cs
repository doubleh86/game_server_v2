using NetworkProtocols;
using NetworkProtocols.Socket.CommonCommand;
using NotifyServer.Helpers;
using NotifyServer.Models.NetworkModels;
using SuperSocket.Command;
using SuperSocket.Server.Abstractions.Session;

namespace NotifyServer.NetworkCommand;

[Command(Key = PingCommand.PingCommandId)]
public class Ping : IAsyncCommand<NetworkPackage>
{
    public async ValueTask ExecuteAsync(IAppSession session, NetworkPackage package, CancellationToken cancellationToken)
    {
        var receivedPackage = MemoryPackHelper.Deserialize<PingCommand>(package.Body);
        if(receivedPackage == null)
            return;
        
        var returnCommand = new PongCommand
        {
            SendTimeMilliseconds = receivedPackage.SendTimeMilliseconds
        };

        var sendPackage = NetworkHelper.CreateSendPacket(PongCommand.PongCommandId, returnCommand);
        await session.SendAsync(sendPackage.GetSendBuffer(), cancellationToken);
    }
}