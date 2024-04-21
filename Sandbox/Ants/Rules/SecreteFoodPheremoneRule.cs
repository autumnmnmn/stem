using Sandbox.Ants.Aspects;
using Stem;
using Stem.Aspects;
using Stem.Rules;

namespace Sandbox.Ants.Rules;

public class SecreteFoodPheromoneRule : Rule
{
    protected override Archetype Archetype => Archetype.Create<Position2D, FoundFoodThought>();

    private readonly Random __rng = new();

    protected override void OnTick(int[] entities, IEntityStore store, TickTime time)
    {
        for (int i = 0; i < entities.Length; ++i)
        {
            var ant = entities[i];
            var position = store.GetAspect<Position2D>(ant);
            ref var thought = ref store.GetAspectRef<FoundFoodThought>(ant);

            if (__rng.NextDouble() < 0.01)
            {
                thought.thoughtAge++;
            }

            if ((time.tick + i) % 10 != 0) continue;

            if (thought.thoughtAge >= 100)
            {
                store.Revoke<FoundFoodThought>(ant);
            }
            else
            {
                var pheromone = store.NewEntity();
                store.Assign(pheromone, new Pheromone() { strength = (2000 / (thought.thoughtAge + 1)) });
                store.Assign(pheromone, position);
            }
        }
    }
}
