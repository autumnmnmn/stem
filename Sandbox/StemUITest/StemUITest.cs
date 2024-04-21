using Stem;
using Stem.Aspects;
using Stem.Rules.Rendering;
using Utilities.Launcher;
using OpenTK.Windowing.Desktop;
using OpenTK.Graphics.OpenGL4;
using Stem.Rules;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Stem.Rules.UI;
using Stem.Rules.UI.Rendering;
using Sandbox.ScratchPad;
using System.Text;

namespace Sandbox.StemUITest;

public enum TransmissionCodeX {
        EXIT = 69420,
        COMMAND = 3,
        IMAGE_RESULT = 1
    }

public class StemUITest : IRunnable
{
    public void Run()
    {
        var listener = new SocketListener<TransmissionCodeX>(TransmissionCodeX.EXIT, "/tmp/sd_transmitter.s");

        listener.On(TransmissionCodeX.COMMAND, (contentLength, content) => {
            Console.WriteLine($"RECEIVED A COMMAND WITH {contentLength} BYTES OF DATA:");
            Console.WriteLine(Encoding.ASCII.GetString(content));
        });

        var suit = new StemInstance();

        var sheetRule = new SheetRenderRule<SheetAppearance>("emoji.jpg", 1, 1, s => (s.index, s.scale));

        suit.RegisterRuleBooks(reg =>
        {
            reg.AddRuleBook()
                .WithRules(reg => reg.Register(() => new ClearColorRule()))
                .ApplicableWhen(state => !state.Get<KeyboardState>().IsKeyDown(Keys.Z));
            reg.AddRuleBook()
                .WithRules(reg =>
                {
                    reg.Register<KeyboardWatcherRule>();
                    reg.Register<WanderRule>();
                    reg.Register<UIButtonRule>();
                    reg.Register<SimpleTypesetterRule>();
                });
            reg.AddRuleBook()
                .WithRules(reg =>
                {
                    reg.Register(() => new GLEnableRule(EnableCap.Blend));
                    reg.Register(() => new GLEnableRule(EnableCap.DepthTest));
                    reg.Register(() => sheetRule);
                    reg.Register<UIRectRenderRule>();
                    reg.Register<GlyphRenderRule>();
                    reg.Register<UIGlyphRenderRule>();
                });
        });

        suit.ModifyState((store, state) =>
        {
            Func<string, string> f = a => $"{a}{a}{a}{a}";

            Func<string, int, string> g = (a, n) =>
            {
                for (int i = 0; i < n; ++i)
                {
                    a = f(a);
                }
                return a;
            };

            var test3 = "Test 1 2 3";//g("lol, lmao, ", 5);

            //var rowLength = 120;

            //Ints.ZeroUpUntil(test3.Length).ForEach(i =>
            //{
                //var letter = store.NewEntity();
                //store.Assign<Position2D>(letter, new() { X = -1500 + ((i % rowLength) * 25), Y = 100 - (37.5 * (i - (i % rowLength)) / rowLength) });
                //store.Assign<GlyphAppearance>(letter, new() { glyphIndex = test3[i], depth = 0.75f });
                //store.Assign<WanderBehavior>(letter);
            //});

            var stringEntity = store.NewEntity();
            store.Assign<Position2D>(stringEntity, new() {X = -200, Y = 0});
            store.Assign<SimpleStringAppearance>(stringEntity, new(test3));

            var watcher = new StandardKeyboardWatcher(
                () => store.GetAspect<SimpleStringAppearance>(stringEntity).value,
                value => {
                    ref var appearance = ref store.GetAspectRef<SimpleStringAppearance>(stringEntity);
                    appearance.value = value;
                    appearance.dirty = true;
                }
            );

            store.Assign<KeyboardWatcher>(stringEntity, new(watcher) { isCapturing = true });

            state.Set("stringEntity", stringEntity);

            //var emoji = store.NewEntity();
            //store.Assign<UIRectAppearance>(emoji, new() { scale = 10000 });
            //store.Assign<Position2D>(emoji, new() { X = 5000, Y = -5000 });

            //Ints.ZeroUpUntil(9).ForEach(i =>
            //{
            //    var sprite = store.NewEntity();
            //    store.Assign<Position2D>(sprite, new() { X = -500 + 500 * (i), Y = 500 - 500 * (i - (i % 3)) / 3 });
            //    store.Assign<SheetAppearance>(sprite, new() { index = i, scale = 240});
            //});

            var button = store.NewEntity();
            store.Assign<Position2D>(button, new() { X = 0, Y = 200 });
            store.Assign<UIButtonBehavior>(button, new() { actionName = "testo" });
            store.Assign<UIRectAppearance>(button, new() { xScale = 200, yScale = 100, r = 0, g = 0.8f, b = 0.6f });
            store.Assign<SimpleStringAppearance>(button, new("Button") { ui = true });

            var canvas = store.NewEntity();

            store.Assign<Position2D>(canvas, new() { X = 500, Y = 0 });
            store.Assign<SheetAppearance>(canvas, new() { scale = 250, index = 0 });

            state.Set(new Random(42069));
        });

        suit.CreateAction("testo", (store, state) =>
        {
            if (!state.IsSet<int>("button press count"))
            {
                state.Set("button press count", 0);
            }
            var count = state.Get<int>("button press count");
            state.Set("button press count", ++count);

            var stringEntity = state.Get<int>("stringEntity");
            ref var app = ref store.GetAspectRef<SimpleStringAppearance>(stringEntity);
            app.dirty = true;
            app.spacing = state.Get<Random>().NextDouble() * 10 + 10;
            app.glyphScale = state.Get<Random>().NextDouble() * 10 + 10;
            app.value = $"Button pressed {count} times!";
        });

        listener.On(TransmissionCodeX.IMAGE_RESULT, (contentLength, content) => {
            var w = BitConverter.ToInt32(content, 0);
            var h = BitConverter.ToInt32(content, 4);
            var imageBytes = content.AsSpan()[8..];
            Console.WriteLine($"Received {w}x{h} image...");

            sheetRule.ReplaceTexture(imageBytes, w, h);
        });

        var settings = new NativeWindowSettings()
        {
            Size = new() { X = 1600, Y = 900 },
            Title = "Stem UI Test",
            WindowBorder = OpenTK.Windowing.Common.WindowBorder.Hidden,
            Location = new() { X = 100, Y = 200 }
        };

        listener.Start();

        try {
            suit.RunWindow(new(), settings);
        }
        finally {
            listener.Cancel();
        }
    }
}

public struct WanderBehavior { }

public class WanderRule : Rule
{
    protected override Archetype Archetype => Archetype.Create<WanderBehavior, Position2D>();

    private readonly Random __rng = new();

    protected override void OnTick(int[] entities, IEntityStore store, TickTime time)
    {
        for (int index = 0; index < entities.Length; ++index)
        {
            ref var position = ref store.GetAspectRef<Position2D>(entities[index]);
            var theta = __rng.NextDouble() * 6.28318;
            position.X += Math.Cos(theta) * time.dt * 50;
            position.Y += Math.Sin(theta) * time.dt * 50;
        }
    }
}
