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
    
    private async ValueTask _OnMonsterUpdate(List<MonsterObject> monsters)
    {
        var updateMonsterGroups = new HashSet<MonsterGroup>();
        var dirtyMonsters = monsters.Where(x => x.IsChanged()).ToList();
        if (dirtyMonsters.Count == 0)
            return;
        
        foreach (var monster in dirtyMonsters)
        {
            var oldPosition = monster.GetPosition();
            var newPosition = monster.GetChangePosition();

            if (newPosition == Vector3.Zero || oldPosition == newPosition)
                continue;
            
            var cell = _worldMapInfo.GetCell(oldPosition, monster.GetZoneId());
            var changeCell = _worldMapInfo.GetCell(newPosition, monster.GetZoneId());
            
            if (changeCell == null)
                continue;
            
            monster.UpdatePosition(newPosition, monster.GetZoneId());
            if (cell == changeCell) 
                continue;
            
            changeCell.Enter(monster);
            cell.Leave(monster.GetId());
            
            monster.ResetChanged();
            updateMonsterGroups.Add(monster.GetGroup());
        }

        foreach (var monsterGroup in updateMonsterGroups)
        {
            monsterGroup.UpdateIsAnyMemberInCombat(_worldOwner);
        }
        
        var gameCommand = new MonsterUpdateCommand
        {
            Monsters = dirtyMonsters.Select(x => x.ToPacket()).ToList()
        };
        
        var notify = new GameCommandResponse
        {
            CommandId = (int)GameCommandId.MonsterUpdateCommand,
            CommandData = MemoryPackHelper.Serialize(gameCommand)
        };
        
        var sendPackage = NetworkHelper.CreateSendPacket((int)WorldServerKeys.GameCommandResponse, notify);
        await _worldOwner.GetSessionInfo().SendAsync(sendPackage.GetSendBuffer());
    }
}