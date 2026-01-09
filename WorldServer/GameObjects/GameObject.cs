using System.Numerics;
using NetworkProtocols.Shared.Enums;
using NetworkProtocols.Socket.WorldServerProtocols.GameProtocols;

namespace WorldServer.GameObjects;

public abstract class GameObject
{
    protected readonly long _id;
    private readonly GameObjectType _objectType; // 0: Player, 1: Monster, 2: StaticObject, 3: DynamicObject
    private Vector3 _position;
    private int _zoneId;

    public long GetId() => _id;
    public Vector3 GetPosition() => _position;
    public int GetZoneId() => _zoneId;
    public abstract GameObjectBase ToPacket();
    protected GameObject(long id, int zoneId, Vector3 position, GameObjectType objectType)
    {
        _id = id;
        _position = position;
        _zoneId = zoneId;
        _objectType = objectType;
    }

    public void UpdatePosition(Vector3 position, int zoneId)
    {
        _position = position;
        _zoneId = zoneId;
    }
}