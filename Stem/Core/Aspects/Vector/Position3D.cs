using System.Runtime.InteropServices;

namespace Stem.Aspects;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct Position3
{
    public Vector3 vector;

    public double X
    {
        get => vector.x;
        set => vector.x = value;
    }

    public double Y
    {
        get => vector.y;
        set => vector.y = value;
    }

    public double Z
    {
        get => vector.z;
        set => vector.z = value;
    }
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct Position3f
{
    public Vector3f vector;

    public float X
    {
        get => vector.x;
        set => vector.x = value;
    }

    public float Y
    {
        get => vector.y;
        set => vector.y = value;
    }

    public float Z
    {
        get => vector.z;
        set => vector.z = value;
    }
}
