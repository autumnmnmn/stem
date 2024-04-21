using System.Runtime.InteropServices;

namespace Stem.Aspects;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct Position2D
{
    public Vector2 vector;

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

    public static implicit operator Position2D((double, double) tuple) => new() {X = tuple.Item1, Y = tuple.Item2};

    public static implicit operator (double, double)(Position2D position) => (position.X, position.Y);
}
