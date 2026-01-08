using System.Numerics;

namespace WorldServer.GameObjects;

public abstract class GameObject
{
    protected long _id;
    protected Vector3 _position;
    
    public GameObject(long id, Vector3 position)
    {
        _id = id;
        _position = position;
    }
    
}