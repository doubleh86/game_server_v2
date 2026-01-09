using System.Numerics;
using NetworkProtocols.Shared.Enums;
using NetworkProtocols.Socket.WorldServerProtocols.GameProtocols;

namespace WorldServer.GameObjects;

public class MonsterObject(long id, Vector3 position, int zoneId) 
    : GameObject(id, zoneId, position, GameObjectType.Monster)
{
    private readonly MonsterObjectBase _packet = new();
    // 0 : idle, 1 : move, 2 : target, 3 : attack
    private int _state;
    private int _targetUserIndex;

    public async Task<bool> UpdateStateAsync(int state)
    {
        if (_state == state)
            return false;
       
        _state = state;
        return true;
    }

    public override MonsterObjectBase ToPacket()
    {
        _packet.Id = _id;
        _packet.Position = GetPosition();
        _packet.ZoneId = GetZoneId();
        _packet.State = _state;

        return _packet;
    }

}