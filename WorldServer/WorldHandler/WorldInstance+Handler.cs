using NetworkProtocols;
using NetworkProtocols.Socket;
using NetworkProtocols.Socket.CommonCommand;
using NetworkProtocols.Socket.NotifyServerProtocols;
using NetworkProtocols.Socket.WorldServerProtocols;
using NetworkProtocols.Socket.WorldServerProtocols.GameProtocols;
using NotifyServer.Helpers;
using SuperSocket.Server.Abstractions.Session;
using WorldServer.JobModels;

namespace WorldServer.GameService;

public partial class WorldInstance
{
    private async ValueTask _HandleMove(byte[] commandData)
    {
        var moveCommand = MemoryPackHelper.Deserialize<MoveCommand>(commandData);
        
        _Push(new ActionJob<MoveCommand>(moveCommand, async (data) =>
        {
            Console.WriteLine($"[{userSessionInfo.Identifier}] Move [{data.X}, {data.Y}, {data.Direction}]");
        }));
    }

    private async ValueTask _OnMonsterUpdate()
    {
        var gameCommand = new MonsterUpdateCommand
        {
            Monsters = _monsters.Values.Select(x => x.ToPacket()).ToList()
        };

        var notify = new GameCommandResponse()
        {
            CommandId = (int)GameCommandId.MonsterUpdateCommand,
            CommandData = MemoryPackHelper.Serialize(gameCommand)
        };
        
        var sendPackage = NetworkHelper.CreateSendPacket((int)WorldServerKeys.GameCommandResponse, notify);
        await userSessionInfo.SendAsync(sendPackage.GetSendBuffer());
    }
}