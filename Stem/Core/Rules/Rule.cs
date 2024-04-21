using System;
using Stem.Internal;

namespace Stem.Rules;

public abstract class Rule : IRule
{
    protected abstract Archetype Archetype { get; }

    protected IState Global { get; private set; }

    protected IState Book { get; private set; }

    private Func<EntityManager, int[]> __entityGetter;

    private Func<EntityManager, int[]> EntityGetter
    {
        get
        {
            if (__entityGetter is null)
            {
                __entityGetter = Archetype.GetEntities;
            }
            return __entityGetter;
        }
    }

    protected internal virtual void Setup(IEntityStore store) { }

    protected abstract void OnTick(int[] entities, IEntityStore store, TickTime time);

    protected internal virtual void Cleanup() { }

    void IRule.Cleanup()
        => Cleanup();

    void IRule.OnTick(EntityManager manager, TickTime time)
        => OnTick(EntityGetter(manager), manager, time);

    void IRule.RegisterAspects(IAspectRegistrar aspectRegistrar)
        => Archetype.RegisterAspects(aspectRegistrar);

    void IRule.Setup(IEntityStore store, IState globalState, IState bookState)
    {
        Global = globalState;
        Book = bookState;
        Setup(store);
    }
}
