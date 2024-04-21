using Utilities.Extensions;
using System.Linq;

namespace Stem.Aspects;

public struct LineAppearance
{
    public LineSegment[] segments;

    public LineAppearance()
    { segments = System.Array.Empty<LineSegment>(); }

    public LineAppearance(params LineSegment[] segments)
    {
        this.segments = segments;
    }

    public LineAppearance(params Vector2[] positions)
    {
        segments = positions.Adjacents()
            .Select(pair => new LineSegment(pair.First, pair.Second))
            .ToArray();
    }
}
