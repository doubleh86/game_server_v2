using System.Numerics;
using WorldServer.Network;

namespace WorldServer.GameObjects;

public class PlayerObject : GameObject
{
    private readonly UserSessionInfo _sessionInfo;
    public UserSessionInfo GetSessionInfo() => _sessionInfo;
    public PlayerObject(long id, Vector3 position, UserSessionInfo sessionInfo) : base(id, position)
    {
        _sessionInfo = sessionInfo;
    }
}