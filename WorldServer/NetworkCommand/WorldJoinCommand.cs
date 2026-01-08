using NetworkProtocols.Socket.CommonCommand;
using NetworkProtocols.Socket.NotifyServerProtocols;
using NetworkProtocols.Socket.WorldServerProtocols;
using NotifyServer.Helpers;
using ServerFramework.CommonUtils.Helper;
using SuperSocket.Command;
using SuperSocket.Server.Abstractions.Session;
using WorldServer.GameService;
using WorldServer.Network;
using WorldServer.Services;

namespace WorldServer.NetworkCommand;

[Command(Key = WorldServerKeys.RequestWorldJoin)]
public class WorldJoinCommand(LoggerService loggerService, WorldService worldService) : IAsyncCommand<NetworkPackage>
{
    private readonly LoggerService _loggerService = loggerService;
    private readonly WorldService _worldService = worldService;

    public async ValueTask ExecuteAsync(IAppSession session, NetworkPackage package, CancellationToken cancellationToken)
    {
        if (session is not UserSessionInfo userSessionInfo)
            throw new ArgumentException("Invalid session type");

        if (userSessionInfo.GetWorldInstance() != null)
        {
            throw new Exception("Already joined world");
        }

        var newWorld = await _worldService.CreateWorldInstance(Guid.NewGuid().ToString(), userSessionInfo);
        userSessionInfo.SetWorldInstance(newWorld);

        var response = new WorldJoinCommandResponse()
        {
            Identifier = (long)WorldServerKeys.ResponseWorldJoin,
            RoomId = newWorld.GetRoomId()
        };
        
        var sendPackage = NetworkHelper.CreateSendPacket((int)WorldServerKeys.ResponseWorldJoin, response);
        await session.SendAsync(sendPackage.GetSendBuffer(), cancellationToken);
    }
}