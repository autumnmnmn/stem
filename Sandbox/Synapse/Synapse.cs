using Stem;
using Stem.Rules;
using Utilities.Launcher;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sandbox.Synapse;

public class Synapse : IRunnable
{
    public void Run()
    {
        var stem = new StemInstance();

        stem.RegisterRuleBooks(reg =>
        {
            reg.AddRuleBook()
                .WithRules(reg =>
                {
                    reg.Register<XRule>();
                });
        });

        stem.ModifyState((store, state) =>
        {

        });

        using var _ = stem.Run();

        bool shouldRun = true;

        while(shouldRun)
        {
            if (Console.KeyAvailable)
            {
                var key = Console.ReadKey(true);
                switch (key.Key)
                {
                    case ConsoleKey.Escape:
                        shouldRun = false;
                        break;
                    case ConsoleKey.F:
                        Console.WriteLine(">F");
                        break;
                    default:
                        break;
                }
            }

            stem.ExecuteAllRules(0);
        }

        Console.Write("Press any key to exit... ");
        Console.ReadKey();
    }
}

public class XRule : Rule
{
    protected override Archetype Archetype => Archetype.NoRead;

    protected override void OnTick(int[] entities, IEntityStore store, TickTime time)
    {
        if (time.tick % 10000 > 0) return;

        Console.Write($"== FRAME {time.tick} : dt = {time.dt} ==\n");
    }
}
