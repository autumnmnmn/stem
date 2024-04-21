using System.Runtime.InteropServices;

namespace Stem.Aspects;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct Vector2f
{
    // TODO: more constructors, swizzling, etc

    public float x;
    public float y;

    #region UV
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
    #endregion

    public Vector2f()
    { x = 0; y = 0; }

    public Vector2f(float x, float y)
    { this.x = x; this.y = y; }
}
