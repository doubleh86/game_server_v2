using System.Numerics;
using NetworkProtocols.Shared.Enums;
using NetworkProtocols.Socket.WorldServerProtocols.GameProtocols;

namespace WorldServer.GameObjects;

public abstract class GameObject
{
    protected readonly long _id;
    private readonly GameObjectType _objectType; // 0: Player, 1: Monster, 2: StaticObject, 3: DynamicObject
    private Vector3 _position;
    private Vector3 _changePosition = Vector3.Zero;
    private int _zoneId;
    
    protected bool _isChanged = false;
    public bool IsChanged() => _isChanged;

    public long GetId() => _id;
    public Vector3 GetPosition() => _position;
    public Vector3 GetChangePosition() => _changePosition;
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
        if (_position == position && zoneId == _zoneId)
            return;

        if (position == Vector3.Zero)
            return;
        
        _position = position;
        _zoneId = zoneId;
        
        _UpdateChangePosition(Vector3.Zero);
    }
    
    protected void _UpdateChangePosition(Vector3 changePosition)
    {
        if (changePosition == _position)
            return;
        
        _changePosition = changePosition;
        _isChanged = true;
    }
    
    public void ResetChanged() => _isChanged = false;
}