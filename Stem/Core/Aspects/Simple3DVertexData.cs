using System.Runtime.InteropServices;

namespace Stem.Aspects;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct Simple3DVertexData
{
    public Position3f position;

    public Vector2f uv;

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

    public float U
    {
        get => uv.U;
        set => uv.U = value;
    }

    public float V
    {
        get => uv.V;
        set => uv.V = value;
    }
}
