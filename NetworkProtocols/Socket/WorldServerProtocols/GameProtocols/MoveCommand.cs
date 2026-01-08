using MemoryPack;

namespace NetworkProtocols.Socket.WorldServerProtocols.GameProtocols;


[MemoryPackable]
public partial class MoveCommand : GameCommandBase
{
    public int X { get; set; }
    public int Y { get; set; }
    
    public int Direction { get; set; }
}