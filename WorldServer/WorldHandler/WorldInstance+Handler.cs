using NetworkProtocols.Socket;
using NetworkProtocols.Socket.NotifyServerProtocols;
using NetworkProtocols.Socket.WorldServerProtocols;
using NetworkProtocols.Socket.WorldServerProtocols.GameProtocols;
using NotifyServer.Helpers;
using WorldServer.JobModels;

namespace WorldServer.WorldHandler;

public partial class WorldInstance
{
    private async ValueTask _HandleMove(byte[] commandData)
    {
        var moveCommand = MemoryPackHelper.Deserialize<MoveCommand>(commandData);
        
        _Push(new ActionJob<MoveCommand>(moveCommand, async (data) =>
        {
            Console.WriteLine($"[{_GetUserSessionInfo().Identifier}] Move [{data.X}, {data.Y}, {data.Direction}]");
        }));
    }

}