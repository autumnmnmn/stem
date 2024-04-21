using Utilities.DefaultDictionary;

namespace Sandbox.Ants;

public class PheromoneGrid : DefaultDictionary<(int, int), int>
{
    public PheromoneGrid() : base(0)
    {
    }

    public HashSet<(int, int)> ToBeCleared { get; init; } = new();
}

public struct PheromoneGridMetadata
{
    public double size = 50;

    public PheromoneGridMetadata()
    {
    }
}
