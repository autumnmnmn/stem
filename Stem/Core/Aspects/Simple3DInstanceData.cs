using System.Runtime.InteropServices;

namespace Stem.Aspects;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct Simple3DInstanceData
{
    public Position3f position;

    public float X
    {
        get => position.X;
        set => position.X = value;
    }

    public float Y
    {
        get => position.Y;
        set => position.Y = value;
    }

    public float Z
    {
        get => position.Z;
        set => position.Z = value;
    }

    public float rotation;

    public float scale;
}
