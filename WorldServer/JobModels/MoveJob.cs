// using NetworkProtocols;
// using NetworkProtocols.Socket;
// using NetworkProtocols.Socket.WorldServerProtocols.GameProtocols;
//
// namespace WorldServer.JobModels;
//
// public class MoveJob(byte[] commandData, Action action) : ActionJob(commandData, action)
// {
//     public override void Execute()
//     {
//         var commandPacket = MemoryPackHelper.Deserialize<MoveCommand>(commandData);
//         Console.WriteLine("MoveJob");
//         
//         action?.Invoke();
//     }
//
//     public override async ValueTask ExecuteAsync()
//     {
//         var commandPacket = MemoryPackHelper.Deserialize<MoveCommand>(commandData);
//         Console.WriteLine("MoveJob");
//     }
// }