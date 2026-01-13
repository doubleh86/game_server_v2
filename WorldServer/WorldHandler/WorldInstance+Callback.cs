using System.Numerics;
using NetworkProtocols.Socket;
using NetworkProtocols.Socket.NotifyServerProtocols;
using NetworkProtocols.Socket.WorldServerProtocols;
using NetworkProtocols.Socket.WorldServerProtocols.GameProtocols;
using NotifyServer.Helpers;
using WorldServer.GameObjects;

namespace WorldServer.WorldHandler;

public partial class WorldInstance
{
    private const int _MaxMonsterUpdateCount = 50;
    private async ValueTask _OnMonsterUpdate(List<MonsterObject> monsters)
    {
        var updateMonsterGroups = new HashSet<MonsterGroup>();
        var dirtyMonsters = monsters.Where(x => x.IsChanged()).ToList();
        if (dirtyMonsters.Count == 0)
            return;
        
        foreach (var monster in dirtyMonsters)
        {
            var cell = _worldMapInfo.GetCell(monster.GetPosition(), monster.GetZoneId());
            var changeCell = _worldMapInfo.GetCell(monster.GetChangePosition());
            if (changeCell == null)
            {
                monster.ResetChanged();
                continue;
            }
                
            monster.UpdatePosition(monster.GetChangePosition(), monster.GetChangeRotation(), changeCell.ZoneId);
            monster.ResetChanged();
            
            updateMonsterGroups.Add(monster.GetGroup());
            
            if (cell == changeCell) 
                continue;
            
            changeCell.Enter(monster);
            cell.Leave(monster.GetId());
            
        }

        foreach (var monsterGroup in updateMonsterGroups)
        {
            monsterGroup.UpdateIsAnyMemberInCombat(_worldOwner);
        }
        
        var notify = new GameCommandResponse
        {
            CommandId = (int)GameCommandId.MonsterUpdateCommand,
        };

        for (var i = 0; i < dirtyMonsters.Count; i += _MaxMonsterUpdateCount)
        {
            int count = Math.Min(_MaxMonsterUpdateCount, dirtyMonsters.Count - i);
            var batchMonsters = dirtyMonsters.GetRange(i, count);

            var gameCommand = new MonsterUpdateCommand
            {
                Monsters = batchMonsters.Select(x => x.ToPacket()).ToList()
            };
            
            notify.CommandData = MemoryPackHelper.Serialize(gameCommand);
            var sendPackage = NetworkHelper.CreateSendPacket((int)WorldServerKeys.GameCommandResponse, notify);
            
            await _worldOwner.GetSessionInfo().SendAsync(sendPackage.GetSendBuffer());
        }
        
    }
}