using DbContext.Common;
using NetworkProtocols.Socket;
using NetworkProtocols.Socket.NotifyServerProtocols;
using NetworkProtocols.Socket.WorldServerProtocols;
using NetworkProtocols.Socket.WorldServerProtocols.GameProtocols;
using NotifyServer.Helpers;
using WorldServer.JobModels;

namespace WorldServer.WorldHandler;

public partial class WorldInstance
{
    private ValueTask _HandleMove(byte[] commandData)
    {
        var moveCommand = MemoryPackHelper.Deserialize<MoveCommand>(commandData);
        _Push(new ActionJob<MoveCommand>(moveCommand, async(command)  =>
        {
            var oldCell = _worldMapInfo.GetCell(_worldOwner.GetZoneId(), _worldOwner.GetPosition());
            _worldOwner.UpdatePosition(command.Position, command.ZoneId);
            var newCell = _worldMapInfo.GetCell(_worldOwner.GetZoneId(), _worldOwner.GetPosition());

            if (oldCell == newCell)
                return;
            
            await _worldMapInfo.UpdatePlayerViewAsync(_worldOwner.GetSessionInfo(), oldCell, newCell);
        }));
        
        return ValueTask.CompletedTask;
    }

    private ValueTask _HandleItemUse(byte[] data)
    {
        var useItemCommand = MemoryPackHelper.Deserialize<UseItemCommand>(data);
        if (useItemCommand == null)
            return ValueTask.CompletedTask;

        var commandResponse = new GameCommandResponse
        {
            CommandId = (int)GameCommandId.UseItemResponse
        };
        
        _Push(new ActionJob<UseItemCommand>(useItemCommand, async (command) =>
        {
            await _ItemUseAsync(command, commandResponse);
            Console.WriteLine($"[{_GetUserSessionInfo().Identifier}] Use Item [{command.ItemId}, {command.UseCount}]");
        }));
        
        return ValueTask.CompletedTask;
    }
    
    private async Task _ItemUseAsync(UseItemCommand command, GameCommandResponse commandResponse)
    {
        var useItemCommandResponse = new UseItemCommandResponse
        {
            ItemId = command.ItemId,
            UseCount = command.UseCount
        };

        commandResponse.CommandData = MemoryPackHelper.Serialize(useItemCommandResponse);
        var sendPackage = NetworkHelper.CreateSendPacket((int)WorldServerKeys.GameCommandResponse, commandResponse);
        await _worldOwner.GetSessionInfo().SendAsync(sendPackage.GetSendBuffer());
        _globalDbService.PushJob(_worldOwner.AccountId, async (dbContext) =>
        {
            try
            {
                var result = await dbContext.ItemUseAsync(_worldOwner.AccountId, command.ItemId, command.UseCount);
                _loggerService.Information($"Success Item use : {result}");
            }
            catch (DbContextException e)
            {
                _loggerService.Warning($"Item use failed for Account:{_worldOwner.AccountId}, Item:{command.ItemId}", e);
            }
            catch (Exception e)
            {
                _loggerService.Warning($"Exception {_worldOwner.AccountId}, Item:{command.ItemId} {e.Message}", e);
            }
            
        });
    }


}