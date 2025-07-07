using MemoryPack;

namespace NetworkProtocols.Socket.NotifyServerProtocols.TwoWayModels;

[MemoryPackable]
public partial class TestCommandRequest : IBaseCommand
{
    public long Identifier { get; set; }
    public string TestText { get; set; }
}

[MemoryPackable]
public partial class TestCommandResponse : IBaseCommand
{
    public long Identifier { get; set; }
    public string TestText { get; set; }
}