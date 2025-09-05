using MemoryPack;

namespace NetworkProtocols.Socket.CommonCommand;

[MemoryPackable]
public partial class PingCommand : IBaseCommand
{
    public const int PingCommandId = 102;
    public long Identifier { get; set; }
    public long SendTimeMilliseconds;
    
}


