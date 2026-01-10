using System.Numerics;
using MySqlDataTableLoader.Models;
using MySqlDataTableLoader.Utils.Helper;
using NetworkProtocols.Socket;
using NetworkProtocols.Socket.NotifyServerProtocols;
using NetworkProtocols.Socket.WorldServerProtocols;
using NetworkProtocols.Socket.WorldServerProtocols.GameProtocols;
using NotifyServer.Helpers;
using ServerFramework.CommonUtils.Helper;
using WorldServer.GameObjects;
using WorldServer.Network;

namespace WorldServer.WorldHandler.WorldDataModels;

public class WorldMapInfo : IDisposable
{
    private readonly long _accountId;
    private readonly int _worldMapId;
    
    private readonly Dictionary<int, MapCell[,]> _zoneCells = new();
    private readonly Dictionary<int, MapInfo> _zoneInfos = new();
    
    private List<MonsterGroup> _monsterGroups = new();
    
    public WorldMapInfo(int worldMapId, long accountId)
    {
        _worldMapId = worldMapId;
        _accountId = accountId;
    }
    
    public void Initialize()
    {
        var worldTableData = MySqlDataTableHelper.GetData<WorldInfo>(_worldMapId);
        if(worldTableData == null)
            throw new Exception("World table data is null");
        
        var allZones = MySqlDataTableHelper.GetDataList<MapInfo>()
                                                              .Where(x => x.world_id == _worldMapId);
        _InitializeCells(allZones.ToList());
        _InitializeMonsters();
    }

    private void _AddZone(MapInfo mapInfo)
    {
        if (mapInfo.world_id != _worldMapId)
            return;

        var cells = new MapCell[mapInfo.MaxChunkX, mapInfo.MaxChunkZ];
        for (int x = 0; x < mapInfo.MaxChunkX; x++)
        {
            for (int z = 0; z < mapInfo.MaxChunkZ; z++)
            {
                cells[x, z] = new MapCell(mapInfo.zone_id, x, z);    
            }
        }
        
        _zoneInfos.Add(mapInfo.zone_id, mapInfo);
        _zoneCells.Add(mapInfo.zone_id, cells);
    }

    
    private void _InitializeMonsters()
    {
        var monsterGroupList = MySqlDataTableHelper
            .GetDataList<MonsterTGroup>()
            .Where(x => x.world_id == _worldMapId);

        foreach (var monsterGroup in monsterGroupList)
        {
            var registerMonsterGroup = new MonsterGroup(monsterGroup.Clone() as MonsterTGroup);
            var position = new Vector3(monsterGroup.position_x, 0, monsterGroup.position_z);
            
            registerMonsterGroup.Id = IdGenerator.NextId(_accountId);
            registerMonsterGroup.RoamRadius = 20;
            
            foreach (var monsterId in monsterGroup.MonsterList)
            {
                var cell = GetCell(monsterGroup.zone_id, position);
                if (cell == null)
                    break;
                
                var spawnedMonster = new MonsterObject(monsterId, position, monsterGroup.zone_id, registerMonsterGroup);
                cell.Enter(spawnedMonster);
                registerMonsterGroup.AddMember(spawnedMonster);
            }
            
            if(registerMonsterGroup.MonsterCount < 1)
                continue;
            
            _monsterGroups.Add(registerMonsterGroup);
        }
    }

    private void _InitializeCells(List<MapInfo> mapInfos)
    {
        foreach (var info in mapInfos)
        {
            _AddZone(info.Clone() as MapInfo);
        }
    }

    public void AddObject(GameObject obj)
    {
        var cell = GetCell(obj.GetZoneId(), obj.GetPosition());
        cell?.Enter(obj);
    }

    public MapCell GetCell(int zoneId, Vector3 position)
    {
        if (_zoneCells.TryGetValue(zoneId, out var cells) == false ||
            _zoneInfos.TryGetValue(zoneId, out var mapInfo) == false)
            return null;
        
        int xIdx = Math.Clamp((int)(position.X / mapInfo.chunk_size), 0, mapInfo.MaxChunkX - 1);
        int zIdx = Math.Clamp((int)(position.Z / mapInfo.chunk_size), 0, mapInfo.MaxChunkZ - 1);

        return cells[xIdx, zIdx];
    }

    public List<MapCell> GetNearByCells(int zoneId, int centerX, int centerZ, int range = 1)
    {
        var nearCells = new List<MapCell>();
        
        for (var x = centerX - range; x <= centerX + range; x++)
        {
            for (var z = centerZ - range; z <= centerZ + range; z++)
            {
                var cell = _GetCellByIndex(zoneId, x, z);
                if (cell != null)
                {
                    nearCells.Add(cell);
                }
            }
        }
        
        return nearCells;
    }

    private MapCell _GetCellByIndex(int zoneId, int x, int z)
    {
        if (_zoneCells.TryGetValue(zoneId, out var cells) == false)
            return null;
        
        int maxIdxX = cells.GetLength(0);
        int maxIdxZ = cells.GetLength(1);

        if (x < 0 || x >= maxIdxX || z < 0 || z >= maxIdxZ)
            return null;
        
        return cells[x, z];
    }

    public async ValueTask UpdatePlayerViewAsync(UserSessionInfo sessionInfo, MapCell oldCell, MapCell newCell)
    {
        if (oldCell == null || newCell == null)
            return ;
        
        var oldNearByCells = GetNearByCells(oldCell.ZoneId, oldCell.X, oldCell.Z, range: 2);
        var nearByCells = GetNearByCells(newCell.ZoneId, newCell.X, newCell.Z, range: 2);

        var enterCells = nearByCells.Except(oldNearByCells).ToList();
        var leaveCells = oldNearByCells.Except(nearByCells).ToList();

        foreach (var enterCell in enterCells)
        {
            var objects = enterCell.GetMapObjects().Values.ToList();
            if (objects.Count == 0)
                continue;
            
            List<GameObjectBase> updateObjects = objects.Select(x => x.ToPacket()).ToList();
            var gameCommand = new GameCommandResponse
            {
                CommandId = (int)GameCommandId.SpawnGameObject,
                CommandData = MemoryPackHelper.Serialize<UpdateGameObjects>(new UpdateGameObjects()
                {
                    IsSpawn = true,
                    GameObjects = updateObjects
                })
            };
            
            var sendPackage = NetworkHelper.CreateSendPacket((int)WorldServerKeys.GameCommandResponse, gameCommand);
            await sessionInfo.SendAsync(sendPackage.GetSendBuffer());
        }

        foreach (var leaveCell in leaveCells)
        {
            var objects = leaveCell.GetMapObjects().Values.ToList();
            if (objects.Count == 0)
                continue;
            
            var despawnLists = objects.Select(x => x.ToPacket()).ToList();
            var gameCommand = new GameCommandResponse
            {
                CommandId = (int)GameCommandId.SpawnGameObject,
                CommandData = MemoryPackHelper.Serialize<UpdateGameObjects>(new UpdateGameObjects()
                {
                    IsSpawn = false,
                    GameObjects = despawnLists
                })
            };
            
            var sendPackage = NetworkHelper.CreateSendPacket((int)WorldServerKeys.GameCommandResponse, gameCommand);
            await sessionInfo.SendAsync(sendPackage.GetSendBuffer());
        }
    }

    public void Dispose()
    {
        foreach (var zone in _zoneCells.Values)
        {
            foreach (var cell in zone)
            {
                cell.Dispose();
            }
        }

        // 2. 딕셔너리 메모리 해제
        _monsterGroups.Clear();
        _zoneCells.Clear();
        _zoneInfos.Clear();
    }
}