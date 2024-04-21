using System.Runtime.InteropServices;

namespace Stem.Aspects;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct Vector2
{
    // TODO: more constructors, swizzling, etc

    public double x;
    public double y;

    #region UV
    public double U
    {
        get => x;
        set => x = value;
    }

    public double V
    {
        get => y;
        set => y = value;
    }
    #endregion

    public Vector2()
    { x = 0; y = 0; }

    public Vector2(double x, double y)
    { this.x = x; this.y = y; }



    public static implicit operator Vector2((double, double) tuple) => new() {x = tuple.Item1, y = tuple.Item2};

    public static implicit operator (double, double)(Vector2 vector) => (vector.x, vector.y);
}
