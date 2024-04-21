using Sandbox.Ants.Aspects;
using Stem;
using Stem.Aspects;
using Stem.Rules;

namespace Sandbox.Ants.Rules;

public class PheromoneDegradationRule : Rule
{
    protected override Archetype Archetype => Archetype.Create<Position2D, Pheromone>();

    protected override void Setup(IEntityStore store)
    {
        Global.Set<PheromoneGrid>(new());
        Global.Set<PheromoneGridMetadata>(new());
    }

    protected override void OnTick(int[] entities, IEntityStore store, TickTime time)
    {
        var grid = Global.Get<PheromoneGrid>();
        var gridSize = Global.Get<PheromoneGridMetadata>().size;
        for (int i = 0; i < entities.Length; ++i)
        {
            ref var pheromone = ref store.GetAspectRef<Pheromone>(entities[i]);
            var position = store.GetAspect<Position2D>(entities[i]);
            var gridX = Convert.ToInt32(Math.Floor(position.X / gridSize));
            var gridY = Convert.ToInt32(Math.Floor(position.Y / gridSize));
            if (grid.ToBeCleared.Contains((gridX, gridY)))
            {
                store.DeleteEntity(entities[i]);
                
                grid[(gridX, gridY)] = 0;
                
                continue;
            }
            if (!pheromone.counted)
            {
                pheromone.initialStrength = pheromone.strength;
                grid[(gridX, gridY)] += pheromone.initialStrength;
                pheromone.counted = true;
            }
            if (--pheromone.strength <= 0)
            {
                store.DeleteEntity(entities[i]);
                grid[(gridX, gridY)] -= pheromone.initialStrength;
            }
        }
        grid.ToBeCleared.Clear();
    }
}
