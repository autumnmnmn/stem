using Sandbox.Ants.Aspects;
using Stem;
using Stem.Aspects;
using Stem.Rules;

namespace Sandbox.Ants.Rules;

public class DirectMovementRule : Rule
{
    protected override Archetype Archetype => Archetype.Create<Position2D, DirectMovement, AntPhysique>();

    private readonly Random __rng = new();

    protected override void OnTick(int[] entities, IEntityStore store, TickTime time)
    {
        foreach (int ant in entities)
        {
            var movement = store.GetAspect<DirectMovement>(ant);
            var physique = store.GetAspect<AntPhysique>(ant);
            ref var position = ref store.GetAspectRef<Position2D>(ant);
            var direction = Math.Atan2(movement.targetY - position.Y, movement.targetX - position.X);
            direction += (__rng.NextDouble() - 0.5f);
            position.X += time.dt * 25d * Math.Cos(direction) * physique.movespeed;
            position.Y += time.dt * 25d * Math.Sin(direction) * physique.movespeed;
        }
    }
}
