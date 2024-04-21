using Sandbox.Ants.Aspects;
using Sandbox.Ants.Rules;
using Sandbox.Ants.Rules.Render;
using Stem;
using Stem.Aspects;
using Stem.Rules.Rendering;
using Utilities.Extensions;
using Utilities.Launcher;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Desktop;

namespace Sandbox.Ants;

public class AntSim : IRunnable
{
    public void Run()
    {
        var sim = new StemInstance();

        sim.RegisterRuleBooks(reg =>
        {
            // Simulation
            reg.AddRuleBook()
                .WithRules(reg =>
                {
                    reg.Register<NutrientDataCollectionRule>();
                    reg.Register<PheromoneDegradationRule>();
                    reg.Register<SecreteFoodPheromoneRule>();
                    reg.Register<AntWanderRule>();
                    reg.Register<DirectMovementRule>();
                    reg.Register<EncounterFoodRule>();
                    reg.Register<ArriveHomeWithFoodRule>();
                });

            // Rendering
            reg.AddRuleBook()
                .WithRules(reg =>
                {
                    reg.Register(() => new ClearColorRule(0.1f, 0.1f, 0.1f));
                    reg.Register(() => new GLEnableRule(EnableCap.DepthTest));
                    reg.Register<LineRenderRule>();
                    reg.Register<FoodRenderRule>();
                    reg.Register<AntRenderRule>();
                    reg.Register(() => new GLEnableRule(EnableCap.Blend));
                    reg.Register<PheromoneRenderRule>();
                    reg.Register<PheromoneRegionRenderRule>();
                    reg.Register(() => new GLDisableRule(EnableCap.Blend));
                    reg.Register(() => new GLDisableRule(EnableCap.DepthTest));
                });
        });

        var rng = new Random();

        sim.ModifyState((store, state) =>
        {
            state.Set("MAX_ANTS", 50_000);
            state.Set("MAX_SPEED", 30f);

            var wander = new WanderMovement { direction = -1, driftFrequency = 1 };

            Ints.ZeroUpUntil(10000).ForEach(i =>
            {
                var ant = store.NewEntity();
                store.Assign<Position2D>(ant);
                store.Assign<AntAppearance>(ant, new()
                {
                    red = 1f,
                    green = 1f,
                    blue = 1f
                    //Red = (float) rng.NextDouble(),
                    //Green = (float) rng.NextDouble(),
                    //Blue = (float) rng.NextDouble()
                });
                store.Assign<AntPhysique>(ant);
                store.Assign(ant, wander);
            });

            Ints.ZeroUpUntil(100).ForEach(i =>
            {
                var food = store.NewEntity();
                var r = rng.NextSingle() * 10000 + 50;
                var theta = rng.NextSingle() * MathF.Tau;
                store.Assign<Position2D>(food, new() { X = r * MathF.Cos(theta), Y = r * MathF.Sin(theta) });
                store.Assign<Nutrients>(food, new() { calories = rng.Next((int) r * 10, (int) r * 40) });
                store.Assign<FoodAppearance>(food);
            });

            var lineRenderer = store.NewEntity();
            List<LineSegment> segments = new()
            {
                new LineSegment(-1000, 0, 1000, 0),
                new LineSegment(0, -1000, 0, 1000),
                new LineSegment(-20, -20, 20, 20),
                new LineSegment(-20, 20, 20, -20),
            };
            (-100).UpThrough(100).Select(i => i * 50).ForEach(i =>
            {
                segments.Add(new(-1000, i, 1000, i));
                segments.Add(new(i, -1000, i, 1000));
            });
            store.Assign<LineAppearance>(lineRenderer, new(segments.ToArray()));
        });

        var settings = new NativeWindowSettings()
        {
            Size = new() { X = 1600, Y = 900 },
            Title = "Ants!",
            //SharedContext = GLFWHelper.SharedContext
        };

        sim.RunWindow(new(), settings);
    }
}
