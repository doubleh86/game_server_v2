using System.Numerics;
using DbContext.GameDbContext.DbResultModel;
using NetworkProtocols.Shared.Enums;
using NetworkProtocols.Socket.WorldServerProtocols.GameProtocols;
using WorldServer.Network;

namespace WorldServer.GameObjects;

public class PlayerObject : GameObject
{
    private readonly UserSessionInfo _sessionInfo;
    private PlayerInfoResult _playerInfo;
    
    public UserSessionInfo GetSessionInfo() => _sessionInfo;
    public long AccountId => _playerInfo.account_id;
    public PlayerObject(long id, Vector3 position, UserSessionInfo sessionInfo, int zoneId) 
        : base(id, zoneId, position, GameObjectType.Player)
    {
        _sessionInfo = sessionInfo;
    }

    public void SetPlayerInfo(PlayerInfoResult playerInfo)
    {
        _playerInfo = playerInfo;
        
        var position = new Vector3(_playerInfo.position_x, _playerInfo.position_y, _playerInfo.position_z);
        UpdatePosition(position, 0f, playerInfo.last_zone_id);
    }

    public PlayerInfoResult GetPlayerInfoWithSave(bool isAutoSave = false)
    {
        if(isAutoSave == false)
            return _playerInfo;
        
        // sync 를 맞춘다.
        _playerInfo.last_zone_id = GetZoneId();
        _playerInfo.position_x = GetPosition().X;
        _playerInfo.position_y = GetPosition().Y;
        _playerInfo.position_z = GetPosition().Z;
        
        return _playerInfo;
    }

    public override GameObjectBase ToPacket()
    {
        return new GameObjectBase()
        {
            Id = AccountId,
            ZoneId = GetZoneId(),
            Position = GetPosition(),
            Type = GameObjectType.Player
        };
    }
}