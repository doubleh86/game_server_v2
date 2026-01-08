using NetworkProtocols.Socket.WorldServerProtocols.GameProtocols;

namespace WorldServer.JobModels;

public class ActionJob<T>(T data, Func<T, ValueTask> action) : Job(null) where T : GameCommandBase
{
    private readonly T _data = data;
    private readonly Func<T, ValueTask> _action = action;
    
    public override void Execute() => ExecuteAsync().GetAwaiter().GetResult();
    public override async ValueTask ExecuteAsync() => await _action(_data);
    
}