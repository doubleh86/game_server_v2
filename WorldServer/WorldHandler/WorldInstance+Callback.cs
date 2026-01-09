using System.Numerics;
using NetworkProtocols.Socket;
using NetworkProtocols.Socket.NotifyServerProtocols;
using NetworkProtocols.Socket.WorldServerProtocols;
using NetworkProtocols.Socket.WorldServerProtocols.GameProtocols;
using NotifyServer.Helpers;
using WorldServer.GameObjects;

namespace WorldServer.WorldHandler;

public partial class WorldInstance
{
    
    private async ValueTask _OnMonsterUpdate(List<MonsterObject> monsters)
    {
        var gameCommand = new MonsterUpdateCommand
        {
            Monsters = monsters.Select(x => x.ToPacket()).ToList()
        };
        
        var notify = new GameCommandResponse
        {
            CommandId = (int)GameCommandId.MonsterUpdateCommand,
            CommandData = MemoryPackHelper.Serialize(gameCommand)
        };
        
        var sendPackage = NetworkHelper.CreateSendPacket((int)WorldServerKeys.GameCommandResponse, notify);
        await _worldOwner.GetSessionInfo().SendAsync(sendPackage.GetSendBuffer());
    }
}