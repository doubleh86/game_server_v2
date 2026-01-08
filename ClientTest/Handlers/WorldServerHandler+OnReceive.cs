using NetworkProtocols.Socket;
using NetworkProtocols.Socket.WorldServerProtocols.GameProtocols;

namespace ClientTest.Handlers;

public partial class WorldServerHandler
{
    private void _OnMonsterUpdateCommand(byte[] data)
    {
        var packet = MemoryPackHelper.Deserialize<MonsterUpdateCommand>(data);
        Console.WriteLine("Monster Update");
    }
}