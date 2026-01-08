using System.Numerics;
using NetworkProtocols.Socket.WorldServerProtocols.GameProtocols;
using WorldServer.Network;

namespace WorldServer.GameObjects;

public class MonsterObject(long id, Vector3 position) : GameObject(id, position)
{
    // 0 : idle, 1 : move, 2 : target, 3 : attack
    private int _state;
    private int _targetUserIndex;

    public ValueTask UpdateStateAsync(int state)
    {
        _state = state;
        return ValueTask.CompletedTask;
    }

    public MonsterObjectBase ToPacket()
    {
        return new MonsterObjectBase()
        {
            Id = _id,
            State = _state
        };
    }
}