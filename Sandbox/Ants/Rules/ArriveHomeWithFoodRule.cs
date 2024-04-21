using Sandbox.Ants.Aspects;
using Stem;
using Stem.Aspects;
using Stem.Rules;

namespace Sandbox.Ants.Rules;

public class ArriveHomeWithFoodRule : Rule
{
    protected override Archetype Archetype => Archetype.Create<Position2D, DirectMovement, AntPhysique>();

    protected override void OnTick(int[] entities, IEntityStore store, TickTime time)
    {
        var maxSpeed = Global.Get<float>("MAX_SPEED");

        foreach (int ant in entities)
        {
            var position = store.GetAspect<Position2D>(ant);
            var movement = store.GetAspect<DirectMovement>(ant);
            ref var physique = ref store.GetAspectRef<AntPhysique>(ant);
            if (Math.Abs(position.X - movement.targetX) < 1.1 * physique.movespeed && Math.Abs(position.Y - movement.targetY) < 1.1 * physique.movespeed)
            {
                store.Revoke<DirectMovement>(ant);
                store.Assign<WanderMovement>(ant, new() { direction = -1, driftFrequency = -1 });
                if (store.HasAspect<FoundFoodThought>(ant))
                {
                    store.Revoke<FoundFoodThought>(ant);
                }
                if (false.Equals(true)) // prevent unreachable code warning //(entities.Length < Global.Get<int>("MAX_ANTS"))
                {
                    var baby = store.NewEntity();
                    store.Assign(baby, position);
                    store.Assign(baby, new WanderMovement() { direction = -1, driftFrequency = -1 });
                    store.Assign(baby, new AntAppearance() { red = 1, green = 1, blue = 1 });
                    store.Assign<AntPhysique>(baby);
                    physique.movespeed *= 1.05f;
                } 
                else
                {
                    physique.movespeed *= 1.2f;
                }
                if (physique.movespeed > maxSpeed)
                {
                    physique.movespeed = maxSpeed;
                }
            }
        }
    }
}
