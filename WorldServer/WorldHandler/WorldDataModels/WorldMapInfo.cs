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
    private const float WorldGridSize = 100; // 100m 격자 구성
    private readonly LoggerService _loggerService;
    
    private readonly long _accountId;
    private readonly int _worldMapId;
    
    private readonly Dictionary<int, MapCell[,]> _zoneCells = new();
    private readonly Dictionary<int, MapInfo> _zoneInfos = new();
    
    private readonly List<MonsterGroup> _monsterGroups = new();
    private readonly Dictionary<long, List<int>> _worldZoneGrid = new();
    
    public WorldMapInfo(int worldMapId, long accountId, LoggerService loggerService)
    {
        _worldMapId = worldMapId;
        _accountId = accountId;
        
        _loggerService = loggerService;
    }
    
    public void Initialize()
    {
        var worldTableData = MySqlDataTableHelper.GetData<WorldInfo>(_worldMapId);
        if(worldTableData == null)
            throw new Exception("World table data is null");
        
        var allZones = MySqlDataTableHelper.GetDataList<MapInfo>()
                                                              .Where(x => x.world_id == _worldMapId);
        _InitializeCells(allZones.ToList());
        _IntializeWorldZoneGrid();
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
                cells[x, z] = new MapCell(mapInfo.zone_id, x, z, mapInfo.WorldOffset, mapInfo.chunk_size);    
            }
        }
        
        _zoneInfos.Add(mapInfo.zone_id, mapInfo);
        _zoneCells.Add(mapInfo.zone_id, cells);
    }

    private void _IntializeWorldZoneGrid()
    {
        _worldZoneGrid.Clear();
        foreach (var (zoneId, info) in _zoneInfos)
        {
            float minX = info.world_offset_x;
            float maxX = minX + (info.chunk_size * info.MaxChunkX);
            float minZ = info.world_offset_z;
            float maxZ = minZ + (info.chunk_size * info.MaxChunkZ);

            int startX = (int)Math.Floor(minX / WorldGridSize);
            int endX = (int)Math.Ceiling(maxX / WorldGridSize) - 1;
            int startZ = (int)Math.Floor(minZ / WorldGridSize);
            int endZ = (int)Math.Ceiling(maxZ / WorldGridSize) - 1;
            
            endX = Math.Max(endX, startX);
            endZ = Math.Max(endZ, startZ);
            
            for (int gx = startX; gx <= endX; gx++)
            {
                for (int gz = startZ; gz <= endZ; gz++)
                {
                    var gridKey = _GetGridKey(gx, gz);
                    if (!_worldZoneGrid.TryGetValue(gridKey, out var zones))
                    {
                        zones = new List<int>(capacity: 4);
                        _worldZoneGrid[gridKey] = zones;
                    }

                    if (zones.Contains(zoneId) == false)
                        zones.Add(zoneId);
                }
            }
        }
    }

    private long _GetGridKey(long gridX, long gridZ)
    {
        return (gridX << 32) | (gridZ & 0xFFFFFFFFL);
    }
    
    private void _InitializeMonsters()
    {
        var monsterGroupList = MySqlDataTableHelper
            .GetDataList<MonsterTGroup>()
            .Where(x => x.world_id == _worldMapId);

        foreach (var monsterGroup in monsterGroupList)
        {
            var zoneId = _FindZoneByWorldPosition(monsterGroup.AnchorPosition);
            if (zoneId == -1)
            {
                _loggerService.Warning($"Can't find zone for monster {monsterGroup.monster_group_id}");
                continue;
            }
            
            var registerMonsterGroup = new MonsterGroup(monsterGroup.Clone() as MonsterTGroup, zoneId);
            var position = new Vector3(monsterGroup.position_x, monsterGroup.position_y, monsterGroup.position_z);
            
            registerMonsterGroup.Id = IdGenerator.NextId(_accountId);
            registerMonsterGroup.RoamRadius = 20;
            
            foreach (var monsterId in monsterGroup.MonsterList)
            {
                var cell = _GetCell(position);
                if (cell == null)
                    break;
                
                var spawnedMonster = new MonsterObject(monsterId, position, zoneId, registerMonsterGroup);
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
        var cell = GetCell(obj.GetPosition());
        cell?.Enter(obj);
    }

    public MapCell GetCell(Vector3 worldPosition, int zoneId = 0)
    {
        if (zoneId != 0 && _zoneInfos.TryGetValue(zoneId, out var mapInfo) == true)
        {
            var minX = mapInfo.world_offset_x;
            var maxX = mapInfo.world_offset_x + mapInfo.chunk_size * mapInfo.MaxChunkX;
            var minZ = mapInfo.world_offset_z;
            var maxZ = mapInfo.world_offset_z + mapInfo.chunk_size * mapInfo.MaxChunkZ;

            if (worldPosition.X >= minX && worldPosition.X < maxX &&
                worldPosition.Z >= minZ && worldPosition.Z < maxZ)
            {
                int xIdx = (int)((worldPosition.X - mapInfo.world_offset_x) / mapInfo.chunk_size);
                int zIdx = (int)((worldPosition.Z - mapInfo.world_offset_z) / mapInfo.chunk_size);
                return _GetCellByIndex(zoneId, xIdx, zIdx);
            }
        }

        return _GetCell(worldPosition);
    }

    private MapCell _GetCell(Vector3 worldPosition)
    {
        var findZoneId = _FindZoneByWorldPosition(worldPosition);
        if(_zoneInfos.TryGetValue(findZoneId, out var mapInfo) == false)
            return null;
        
        int xIdx = (int)((worldPosition.X - mapInfo.world_offset_x) / mapInfo.chunk_size);
        int zIdx = (int)((worldPosition.Z - mapInfo.world_offset_z) / mapInfo.chunk_size);

        return _GetCellByIndex(findZoneId, xIdx, zIdx);
    }

    private int _FindZoneByWorldPosition(Vector3 worldPosition)
    {
        var gx = (long)Math.Floor(worldPosition.X / WorldGridSize);
        var gz = (long)Math.Floor(worldPosition.Z / WorldGridSize);
        var gridKey = _GetGridKey(gx, gz);

        if (_worldZoneGrid.TryGetValue(gridKey, out var candidateZones) == false)
            return -1;
        
        foreach(var zoneId in candidateZones)
        {
            if(_zoneInfos.TryGetValue(zoneId, out var zoneEntry) == false)
                continue;
            
            var minX = zoneEntry.world_offset_x;
            var maxX = zoneEntry.world_offset_x + zoneEntry.chunk_size * zoneEntry.MaxChunkX;
            var minZ = zoneEntry.world_offset_z;
            var maxZ = zoneEntry.world_offset_z + zoneEntry.chunk_size * zoneEntry.MaxChunkZ;

            if (worldPosition.X >= minX && worldPosition.X < maxX &&
                worldPosition.Z >= minZ && worldPosition.Z < maxZ)
            {
                return zoneId;
            }
        }
        
        return -1;
    }

    public List<MapCell> GetWorldNearByCells(int zoneId, Vector3 worldPosition, int range = 1)
    {
        var centerCell = GetCell(worldPosition, zoneId);
        if (centerCell == null)
            return [];
        
        var baseZoneId = centerCell.ZoneId;
        var nearCells = new List<MapCell>();
        for (var x = centerCell.X - range; x <= centerCell.X + range; x++)
        {
            for (var z = centerCell.Z - range; z <= centerCell.Z + range; z++)
            {
                var cell = _GetCellByIndex(baseZoneId, x, z);
                if (cell != null)
                {
                    nearCells.Add(cell);
                    continue;
                }
                
                var neighborCell = _GetNeighborZoneCell(baseZoneId, x, z);
                if (neighborCell == null)
                    continue;
                
                nearCells.Add(neighborCell);
            }
        }
        
        return nearCells;
    }

    private MapCell _GetNeighborZoneCell(int zoneId, int xIdx, int zIdx)
    {
        if (_zoneInfos.TryGetValue(zoneId, out var mapInfo) == false)
            return null;
        
        float worldX = mapInfo.world_offset_x + (xIdx * mapInfo.chunk_size) + (mapInfo.chunk_size * 0.5f);
        float worldZ = mapInfo.world_offset_z + (zIdx * mapInfo.chunk_size) + (mapInfo.chunk_size * 0.5f);
        
        var targetWorldPosition = new Vector3(worldX, 0, worldZ);
        int targetZoneId = _FindZoneByWorldPosition(targetWorldPosition);
        if (targetZoneId == -1 || targetZoneId == zoneId)
            return null;
        
        return GetCell(targetWorldPosition, targetZoneId);
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
        
        var oldNearByCells = GetWorldNearByCells(oldCell.ZoneId, oldCell.WorldPosition, range: 2);
        var nearByCells = GetWorldNearByCells(newCell.ZoneId, newCell.WorldPosition, range: 2);

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