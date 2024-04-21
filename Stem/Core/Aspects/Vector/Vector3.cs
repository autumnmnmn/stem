using System.Runtime.InteropServices;

namespace Stem.Aspects;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct Vector3
{
    // TODO: more constructors, swizzling, etc

    public double x;
    public double y;
    public double z;

    #region UVW
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

    public double W
    {
        get => z;
        set => z = value;
    }
    #endregion

    #region RGB
    public double R
    {
        get => x;
        set => x = value;
    }

    public double G
    {
        get => y;
        set => y = value;
    }

    public double B
    {
        get => z;
        set => z = value;
    }
    #endregion

    public Vector3()
    { x = 0; y = 0; z = 0; }

    public Vector3(double x, double y, double z)
    { this.x = x; this.y = y; this.z = z; }
}
