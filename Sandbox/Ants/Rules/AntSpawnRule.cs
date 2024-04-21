using Sandbox.Ants.Aspects;
using Stem;
using Stem.Aspects;
using Stem.Rules;
using Utilities.Extensions;

namespace Sandbox.Ants.Rules;

public class AntSpawnRule : Rule
{
    protected override Archetype Archetype => Archetype.NoRead;

    protected override void OnTick(int[] entities, IEntityStore store, TickTime time)
    {
        var rng = new Random();
        var position = new Position2D { X = 0, Y = 0 };
        var wander = new WanderMovement { direction = -1, driftFrequency = 1 };

        0.Once().ForEach(_ =>
        {
            int ant = store.NewEntity();
            store.Assign(ant, position);
            store.Assign(ant, wander);
            store.Assign<AntAppearance>(ant, new()
            {
                red = (float) rng.NextDouble(),
                green = (float) rng.NextDouble(),
                blue = (float) rng.NextDouble()
            });
        });
    }
}
