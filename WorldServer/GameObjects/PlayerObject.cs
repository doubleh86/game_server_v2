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
    }


    public override GameObjectBase ToPacket()
    {
        throw new NotImplementedException();
    }
}