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

    private void _OnItemUseCommand(byte[] data)
    {
        var packet = MemoryPackHelper.Deserialize<UseItemCommand>(data);
        Console.WriteLine($"Item Use {packet.ItemId}");
    }
}