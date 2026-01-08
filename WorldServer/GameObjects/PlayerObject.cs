using System.Numerics;
using DbContext.GameDbContext.DbResultModel;
using WorldServer.Network;

namespace WorldServer.GameObjects;

public class PlayerObject : GameObject
{
    private readonly UserSessionInfo _sessionInfo;
    private PlayerInfoResult _playerInfo;
    public UserSessionInfo GetSessionInfo() => _sessionInfo;
    public long AccountId => _playerInfo.account_id;
    public PlayerObject(long id, Vector3 position, UserSessionInfo sessionInfo) : base(id, position)
    {
        _sessionInfo = sessionInfo;
    }

    public void SetPlayerInfo(PlayerInfoResult playerInfo)
    {
        _playerInfo = playerInfo;
    }
}