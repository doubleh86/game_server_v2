using DbContext.Common;
using NetworkProtocols.Socket;
using NetworkProtocols.Socket.WorldServerProtocols.GameProtocols;
using WorldServer.JobModels;

namespace WorldServer.WorldHandler;

public partial class WorldInstance
{
    private const int _MaxViewUpdateBatchCount = 100;
    private int _isChangingWorld;
    private void _RegisterGameHandler()
    {
        _commandHandlers.Add(GameCommandId.MoveCommand, _HandleMove);
        _commandHandlers.Add(GameCommandId.UseItemCommand, _HandleItemUse);
        _commandHandlers.Add(GameCommandId.ChangeWorldCommand, _HandleChangeWorld);
    }

    private ValueTask _HandleChangeWorld(byte[] data)
    {
        var changeWorldCommand = MemoryPackHelper.Deserialize<ChangeWorldCommand>(data);
        if (changeWorldCommand == null)
            return ValueTask.CompletedTask;
        
        _Push(new ActionJob<ChangeWorldCommand>(changeWorldCommand, _OnChangeWorldCommand));
        return ValueTask.CompletedTask;
    }

    private ValueTask _HandleMove(byte[] commandData)
    {
        if(Volatile.Read(ref _isChangingWorld) == 1)
            return ValueTask.CompletedTask;
        
        var moveCommand = MemoryPackHelper.Deserialize<MoveCommand>(commandData);
        if(moveCommand == null)
            return ValueTask.CompletedTask;
        
        _Push(new ActionJob<MoveCommand>(moveCommand, _OnMoveCommand));
        return ValueTask.CompletedTask;
    }
    
    
    private ValueTask _HandleItemUse(byte[] data)
    {
        var useItemCommand = MemoryPackHelper.Deserialize<UseItemCommand>(data);
        if (useItemCommand == null)
            return ValueTask.CompletedTask;

        
        _Push(new ActionJob<UseItemCommand>(useItemCommand, async (command) =>
        {
            await _ItemUseAsync(command);
            Console.WriteLine($"[{_GetUserSessionInfo().Identifier}] Use Item [{command.ItemId}, {command.UseCount}]");
        }));
        
        return ValueTask.CompletedTask;
    }
    
    private async Task _ItemUseAsync(UseItemCommand command)
    {
        var useItemCommandResponse = new UseItemCommandResponse
        {
            ItemId = command.ItemId,
            UseCount = command.UseCount
        };

        await _SendGameCommandPacket(GameCommandId.UseItemResponse, MemoryPackHelper.Serialize(useItemCommandResponse));
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