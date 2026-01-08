using NetworkProtocols;
using NetworkProtocols.Socket;
using NetworkProtocols.Socket.CommonCommand;
using NotifyServer.Helpers;
using SuperSocket.Command;
using SuperSocket.Server.Abstractions.Session;
using WorldServer.Network;

namespace WorldServer.NetworkCommand;

[Command(Key = NetworkProtocols.Socket.CommonCommand.PingCommand.PingCommandId)]
public class PingCommand : IAsyncCommand<NetworkPackage>
{
    public async ValueTask ExecuteAsync(IAppSession session, NetworkPackage package, CancellationToken cancellationToken)
    {
        var receivedPackage = MemoryPackHelper.Deserialize<NetworkProtocols.Socket.CommonCommand.PingCommand>(package.Body);
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