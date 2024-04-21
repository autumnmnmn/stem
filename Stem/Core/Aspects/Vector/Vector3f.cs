using System.Runtime.InteropServices;

namespace Stem.Aspects;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct Vector3f
{
    // TODO: more constructors, swizzling, etc

    public float x;
    public float y;
    public float z;

    #region UVW
    public float U
    {
        get => x;
        set => x = value;
    }

    public float V
    {
        get => y;
        set => y = value;
    }

    public float W
    {
        get => z;
        set => z = value;
    }
    #endregion

    #region RGB
    public float R
    {
        get => x;
        set => x = value;
    }

    public float G
    {
        get => y;
        set => y = value;
    }

    public float B
    {
        get => z;
        set => z = value;
    }
    #endregion

    public Vector3f()
    { x = 0; y = 0; z = 0; }

    public Vector3f(float x, float y, float z)
    { this.x = x; this.y = y; this.z = z; }
}
