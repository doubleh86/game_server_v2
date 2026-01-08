using MemoryPack;

namespace NetworkProtocols.Socket.WorldServerProtocols.GameProtocols;

[MemoryPackable]
public partial class MonsterObjectBase
{
    public long Id { get; set; }
    public int State { get; set; }
}

[MemoryPackable]
public partial class MonsterUpdateCommand : GameCommandBase
{
    public List<MonsterObjectBase> Monsters { get; set; }
}