using NetworkProtocols.Socket;
using NetworkProtocols.Socket.WorldServerProtocols.GameProtocols;

namespace ClientTest.Handlers;

public partial class WorldServerHandler
{
    private void _OnMonsterUpdateCommand(byte[] data)
    {
        var packet = MemoryPackHelper.Deserialize<MonsterUpdateCommand>(data);
        
        Console.WriteLine($"Monster Update : {packet.Monsters.Count}");
    }

    private void _OnItemUseCommand(byte[] data)
    {
        var packet = MemoryPackHelper.Deserialize<UseItemCommand>(data);
        Console.WriteLine($"Item Use {packet.ItemId}");
    }

    private void _OnSpawnGameObject(byte[] data)
    {
        var packet = MemoryPackHelper.Deserialize<UpdateGameObjects>(data);
        Console.WriteLine($"Spawn GameObject {packet.GameObjects.Count} | Spawn Type : {packet.IsSpawn}");
    }
}