using System.Numerics;
using DbContext.Common;
using MySqlDataTableLoader.Models;
using MySqlDataTableLoader.Utils.Helper;
using NetworkProtocols.Socket;
using NetworkProtocols.Socket.NotifyServerProtocols;
using NetworkProtocols.Socket.WorldServerProtocols;
using NetworkProtocols.Socket.WorldServerProtocols.GameProtocols;
using NotifyServer.Helpers;
using WorldServer.JobModels;
using WorldServer.Utils;
using WorldServer.WorldHandler.WorldDataModels;

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
        
        _Push(new ActionJob<ChangeWorldCommand>(changeWorldCommand, async(command)  =>
        {
            if (Interlocked.Exchange(ref _isChangingWorld, 1) == 1)
                return;

            try
            {
                if(MySqlDataTableHelper.GetData<WorldInfo>(command.WorldId) == null)
                    throw new WorldServerException(WorldErrorCode.WrongPacket, $"World not found {command.WorldId}");
                
                _worldMapInfo.ClearWorld();
                _worldMapInfo.Initialize(command.WorldId);

                var spawnPosition = new Vector3(40, 0, 40);
                var spawnCell = _worldMapInfo.GetCell(spawnPosition);
                if (spawnCell == null)
                {
                    throw new WorldServerException(WorldErrorCode.WrongPacket, $"Spawn cell not found for position {spawnPosition}");
                }

                _worldOwner.UpdatePosition(spawnPosition, 0, spawnCell.ZoneId);
                spawnCell.Enter(_worldOwner);

                var response = new ChangeWorldCommandResponse()
                               {
                                   WorldId = _worldMapInfo.GetWorldMapId(),
                                   Player = _worldOwner.ToPacket(),
                               };

                await _SendGameCommandPacket(GameCommandId.ChangeWorldResponse, MemoryPackHelper.Serialize(response));
            }
            catch (Exception e)
            {
                _loggerService.Error(e.Message, e);
            }
            finally
            {
                Interlocked.Exchange(ref _isChangingWorld, 0);
            }
        }));
        
        return ValueTask.CompletedTask;
    }

    private ValueTask _HandleMove(byte[] commandData)
    {
        if(Volatile.Read(ref _isChangingWorld) == 1)
            return ValueTask.CompletedTask;
        
        var moveCommand = MemoryPackHelper.Deserialize<MoveCommand>(commandData);
        if(moveCommand == null)
            return ValueTask.CompletedTask;
        
        _Push(new ActionJob<MoveCommand>(moveCommand, async(command)  =>
        {
            var oldZoneId = _worldOwner.GetZoneId();
            var oldPosition = _worldOwner.GetPosition();
            var oldCell = _worldMapInfo.GetCell(oldPosition, oldZoneId);
            var newCell = _worldMapInfo.GetCell(command.Position);
            if (newCell == null)
                return;
            
            _worldOwner.UpdatePosition(command.Position, moveCommand.Rotation, newCell.ZoneId);
            if (oldCell == newCell)
            {
                return;
            }
            
            oldCell?.Leave(_worldOwner.GetId());
            newCell.Enter(_worldOwner);
            var (enterCell, leaveCell) = _worldMapInfo.UpdatePlayerView(oldCell, newCell);
            await _SendViewUpdateBatched(isSpawn: true, cells:enterCell);
            await _SendViewUpdateBatched(isSpawn: false, cells:leaveCell);
        }));
        
        return ValueTask.CompletedTask;
    }

    
    private async ValueTask _SendViewUpdateBatched(bool isSpawn, List<MapCell> cells)
    {
        if (cells == null || cells.Count == 0)
            return;
        
        var seen = new HashSet<long>(capacity: cells.Count * 16);
        var batch = new List<GameObjectBase>(capacity: _MaxViewUpdateBatchCount);

        foreach (var cell in cells)
        {
            var objects = cell.GetMapObjects().Values;
            if (objects.Count == 0)
                continue;

            foreach (var obj in objects)
            {
                var id = obj.GetId();

                if (_worldOwner != null && id == _worldOwner.GetId())
                    continue;
                
                if (seen.Add(id) == false)
                    continue;
                
                batch.Add(obj.ToPacket());
                if (batch.Count >= _MaxViewUpdateBatchCount)
                {
                    var commandData = MemoryPackHelper.Serialize<UpdateGameObjects>(new UpdateGameObjects
                    {
                        IsSpawn = isSpawn,
                        GameObjects = batch
                    });
                    
                    await _SendGameCommandPacket(GameCommandId.SpawnGameObject, commandData);
                    batch.Clear();
                }
            }
        }

        if (batch.Count == 0)
        {
            return;
        }

        var remainData = MemoryPackHelper.Serialize<UpdateGameObjects>(new UpdateGameObjects
                                                                       {
                                                                           IsSpawn = isSpawn,
                                                                           GameObjects = batch
                                                                       });
        await _SendGameCommandPacket(GameCommandId.SpawnGameObject, remainData);
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