using Sandbox;

namespace Kira;

public enum LimbID
{
    LEFT_FOOT,
    RIGHT_FOOT,
    LEFT_HAND,
    RIGHT_HAND
}

public class IkLimb
{
    public string name;
    public GameObject limbObject;
    public Vector3 position;
    public Rotation rotation;
    public LimbID limbID;

    public IkLimb(string name, LimbID limbId, GameObject limbObject)
    {
        this.name = name;
        this.limbID = limbId;
        this.limbObject = limbObject;
    }
}