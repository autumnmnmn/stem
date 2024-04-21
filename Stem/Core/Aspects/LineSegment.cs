namespace Stem.Aspects;

public struct LineSegment
{
    public Vector2 start;
    public Vector2 end;

    public LineSegment()
    { start = new(); end = new(); }

    public LineSegment(Vector2 start, Vector2 end)
    { this.start = start; this.end = end; }

    public LineSegment(double startX, double startY, double endX, double endY) : this(new Vector2(startX, startY), new Vector2(endX, endY))
    {
    }
}
