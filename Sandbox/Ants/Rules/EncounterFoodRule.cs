//using Sandbox.Ants.Aspects;
//using Stem;
//using Stem.Aspects;
//using Stem.Rules;
//using System.Collections.Generic;
//
namespace Sandbox.Ants.Rules;

// TODO: Rewrite this rule to work with the new rule system

//public class EncounterFoodRule : MultiRule2
//{
//    protected override Archetype Archetype => Archetype.Create<Position2D, Nutrients>();
//
//    protected override Archetype Archetype2 => Archetype.Create<Position2D, WanderMovement>();
//
//    List<(int, Nutrients, Position2D)> NutrientData;
//
//    protected override void OnTick<StemWindow>(int[] entities, IEntityStore store, TickTime time, StemWindow window)
//    {
//        NutrientData = new();
//        foreach (int entity in entities)
//        {
//            NutrientData.Add(new(entity, store.GetAspect<Nutrients>(entity), store.GetAspect<Position2D>(entity)));
//        }
//    }
//
//    protected override void OnTick2<StemWindow>(int[] entities, IEntityStore store, TickTime time, StemWindow window)
//    {
//        foreach (int ant in entities)
//        {
//            var antPosition = store.GetAspect<Position2D>(ant);
//            foreach (var (foodEntity, nutrients, position) in NutrientData)
//            {
//                var dx = position.X - antPosition.X;
//                var dy = position.Y - antPosition.Y;
//                if (dx * dx + dy * dy < (nutrients.Calories / 333f * nutrients.Calories / 333f))
//                {
//                    store.Revoke<WanderMovement>(ant);
//                    store.Assign<FoundFoodThought>(ant);
//                    store.Assign<DirectMovement>(ant, new () { TargetX = 0, TargetY = 0 });
//                    ref var nutrientsRef = ref store.GetAspectRef<Nutrients>(foodEntity);
//                    nutrientsRef.Calories -= 5;
//                    if (nutrientsRef.Calories < 0)
//                    {
//                        store.DeleteEntity(foodEntity);
//                    }
//                    var pheromone = store.NewEntity();
//                    store.Assign(pheromone, antPosition);
//                    store.Assign(pheromone, new Pheromone() { Strength = 5000 });
//                }
//            }
//        }
//    }
//}

using Sandbox.Ants.Aspects;
using Stem;
using Stem.Aspects;
using Stem.Rules;
using Utilities.DefaultDictionary;

public class NutrientData : List<(int, Nutrients, Position2D)> { }

public class NutrientGrid : DefaultDictionary<(int, int), List<(Position2D, int)>> 
{ 
    public NutrientGrid() : base(() => new()) { }
}

public class NutrientDataCollectionRule : Rule
{
    protected override Archetype Archetype => Archetype.Create<Position2D, Nutrients>();

    protected override void OnTick(int[] entities, IEntityStore store, TickTime time)
    {
        NutrientData data = new();
        NutrientGrid grid = new();
        Book.Set(data);
        Book.Set(grid);
        var gridSize = Global.Get<PheromoneGridMetadata>().size;
        foreach (int entity in entities)
        {
            var position = store.GetAspect<Position2D>(entity);
            data.Add(new(entity, store.GetAspect<Nutrients>(entity), position));
            grid[Gridify(position, gridSize)].Add((position, entity));
        }
    }
    private static (int, int) Gridify(Position2D p, double gridSize)
    {
        // [TODO] Refactor!!!
        return (Convert.ToInt32(Math.Floor(p.X / gridSize)), Convert.ToInt32(Math.Floor(p.Y / gridSize)));
    }
}

public class EncounterFoodRule : Rule
{
    protected override Archetype Archetype => Archetype.Create<Position2D, WanderMovement>();

    protected override void OnTick(int[] entities, IEntityStore store, TickTime time)
    {
        var data = Book.Get<NutrientData>();
        foreach (int ant in entities)
        {
            var antPosition = store.GetAspect<Position2D>(ant);
            var physique = store.GetAspect<AntPhysique>(ant);
            foreach (var (foodEntity, nutrients, position) in data)
            {
                var dx = position.X - antPosition.X;
                var dy = position.Y - antPosition.Y;
                if (dx * dx + dy * dy < (nutrients.calories / 333f * nutrients.calories / 333f) + physique.movespeed + 10)
                {
                    store.Revoke<WanderMovement>(ant);
                    store.Assign<FoundFoodThought>(ant);
                    store.Assign<DirectMovement>(ant, new () { targetX = 0, targetY = 0 });
                    ref var nutrientsRef = ref store.GetAspectRef<Nutrients>(foodEntity);
                    nutrientsRef.calories -= (int)(5 + physique.movespeed);
                    if (nutrientsRef.calories < 0)
                    {
                        store.DeleteEntity(foodEntity);
                    }
                    var pheromone = store.NewEntity();
                    store.Assign(pheromone, antPosition);
                    store.Assign(pheromone, new Pheromone() { strength = 5000 });
                }
            }
        }
    }
}
