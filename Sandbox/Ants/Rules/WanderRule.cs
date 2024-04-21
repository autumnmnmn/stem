using Sandbox.Ants.Aspects;
using Stem;
using Stem.Aspects;
using Stem.Rules;

namespace Sandbox.Ants.Rules;

public class AntWanderRule : Rule
{
    private readonly Random __rng = new();

    private static List<(int, int)> DirectionCoords { get; } = new();

    private static Dictionary<(int, int), float?> DirectionsByCoord { get; } = new();

    private const int SIGHT_DISTANCE = 1;

    public AntWanderRule()
    {
        for (int i = -SIGHT_DISTANCE; i <= SIGHT_DISTANCE; ++i)
        {
            for (int j = -SIGHT_DISTANCE; j <= SIGHT_DISTANCE; ++j)
            {
                if (i == 0 && j == 0)
                {
                    continue;
                }
                DirectionCoords.Add((i, j));
                DirectionsByCoord[(i, j)] = MathF.Atan2(j, i);
            }
        }
        DirectionsByCoord[(0, 0)] = null;
    }

    protected override Archetype Archetype => Archetype.Create<Position2D, WanderMovement, AntPhysique>();

    protected override void OnTick(int[] entities, IEntityStore store, TickTime time)
    {
        for (int i = 0; i < entities.Length; ++i)
        {
            ref var pos = ref store.GetAspectRef<Position2D>(entities[i]);
            ref var wander = ref store.GetAspectRef<WanderMovement>(entities[i]);
            var gridSize = Global.Get<PheromoneGridMetadata>().size;
            if (wander.direction == -1 || time.tick % (wander.driftFrequency) == 0)
            {
                wander.direction = (float) (__rng.NextDouble() * 6.28318);
                wander.driftFrequency = __rng.Next(1, 500);
            }
            var gridified = Gridify(pos, gridSize);
            var nutrients = Book.Get<NutrientGrid>();
            var localNutrients = nutrients[gridified];
            if (localNutrients.Any())
            {
                var target = localNutrients.First();
                wander.direction = (float) Math.Atan2(target.Item1.Y - pos.Y, target.Item1.X - pos.X);
            }
            else
            {
                var coords = DirectionCoords.Select(offset => Add(offset, gridified));

                localNutrients = coords.SelectMany(coord => nutrients[coord]).ToList();

                if (localNutrients.Any())
                {
                    var target = localNutrients[__rng.Next(localNutrients.Count)];
                    wander.direction = (float) Math.Atan2(target.Item1.Y - pos.Y, target.Item1.X - pos.X);
                }
                else
                {
                    var (pheromoneDirection, pheromoneCoordDirection) = DirectionOfGreatestpheromone(gridified);
                    if (pheromoneDirection is not null)
                    {
                        var currentX = (pos.X / gridSize) - gridified.Item1;
                        var targetX = pheromoneCoordDirection!.Value.Item1 + 0.5f;
                        var dx = targetX - currentX;
                        var currentY = (pos.Y / gridSize) - gridified.Item2;
                        var targetY = pheromoneCoordDirection!.Value.Item2 + 0.5f;
                        var dy = targetY - currentY;

                        pheromoneDirection = MathF.Atan2((float) dy, (float) dx);

                        //if (MathF.Abs(wander.Direction - pheromoneDirection.Value) < 3.14159)
                        {
                            //wander.Direction = (wander.Direction * 10f + pheromoneDirection.Value) / 11f;
                            var randOffset = __rng.NextDouble() * 6.28318;
                            var bias = 8f;
                            var sineAvg = (Math.Sin(wander.direction - randOffset) * bias + Math.Sin(pheromoneDirection.Value - randOffset)) / (bias + 1);
                            var cosineAvg = (Math.Cos(wander.direction - randOffset) * bias + Math.Cos(pheromoneDirection.Value - randOffset)) / (bias + 1);
                            wander.direction = (float) (Math.Atan2(sineAvg, cosineAvg) + randOffset);
                        }
                    }
                    else
                    {
                        var grid = Global.Get<PheromoneGrid>();
                        grid.ToBeCleared.Add(gridified);
                    }
                }
            }

            
            var speed = store.GetAspect<AntPhysique>(entities[i]).movespeed;
            pos.X += time.dt * MathF.Cos(wander.direction) * 25d * speed;
            pos.Y += time.dt * MathF.Sin(wander.direction) * 25d * speed;
        }
    }

    private static (int, int) Gridify(Position2D p, double gridSize)
    {
        // [TODO] Refactor!!!
        return (Convert.ToInt32(Math.Floor(p.X / gridSize)), Convert.ToInt32(Math.Floor(p.Y / gridSize)));
    }

    private (float?, (int, int)?) DirectionOfGreatestpheromone((int, int) center)
    {
        float? direction = null;
        (int, int)? coordDirection = null;
        float greatestpheromone = 0;
        var grid = Global.Get<PheromoneGrid>();

        foreach (var coordOffset in DirectionsByCoord.Keys)
        {
            float value = grid[Add(center, coordOffset)];
            if (value > greatestpheromone)
            {
                greatestpheromone = value;
                direction = DirectionsByCoord[coordOffset];
                coordDirection = coordOffset;
            }
        }

        return (direction, coordDirection);
    }

    private static (int, int) Add((int, int) a, (int, int) b)
    {
        return (a.Item1 + b.Item1, a.Item2 + b.Item2);
    }
}
