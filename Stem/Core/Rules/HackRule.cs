using System;

namespace Stem.Rules;

public static class HackExtension {
    public static void Hack(this IRuleRegistrar reg, Action<IEntityStore, IState, TickTime> onTick) 
        => reg.Register<HackRule>(() => new(onTick));
}

public class HackRule : Rule
{
    protected override Archetype Archetype => Archetype.NoRead;

    private readonly Action<IEntityStore, IState, TickTime> __onTick; 

    public HackRule(Action<IEntityStore, IState, TickTime> onTick) {
        __onTick = onTick ?? throw new ArgumentNullException(nameof(onTick));
    }

    protected override void OnTick(int[] entities, IEntityStore store, TickTime time)
    {
        __onTick(store, Global, time);
    }
}
