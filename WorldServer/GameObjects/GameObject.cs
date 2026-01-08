using System.Numerics;

namespace WorldServer.GameObjects;

public abstract class GameObject
{
    protected readonly long _id;
    protected Vector3 _position;

    protected GameObject(long id, Vector3 position)
    {
        _id = id;
        _position = position;
    }

    public void UpdatePosition(Vector3 position)
    {
        _position = position;
    }
}