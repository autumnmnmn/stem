using System;
using Stem.Internal;

namespace Stem.Rules;

public sealed class Archetype
{
    internal Func<EntityManager, int[]> GetEntities;

    internal Action<IAspectRegistrar> RegisterAspects;

    private Archetype(Func<EntityManager, int[]> getEntities, Action<IAspectRegistrar> registerAspects)
    {
        GetEntities = getEntities;
        RegisterAspects = registerAspects;
    }

    public static Archetype NoRead { get; } = new Archetype(_ => Array.Empty<int>(), _ => { });

    public static Archetype Create<T1>() where T1 : struct
    {
        return new Archetype(manager => manager.GetEntities<T1>(),
            registrar => registrar.Register<T1>());
    }

    public static Archetype Create<T1, T2>() where T1 : struct where T2 : struct
    {
        return new Archetype(manager => manager.GetEntities<T1, T2>(),
            registrar =>
            {
                registrar.Register<T1>();
                registrar.Register<T2>();
            });
    }

    public static Archetype Create<T1, T2, T3>() where T1 : struct where T2 : struct where T3 : struct
    {
        return new Archetype(manager => manager.GetEntities<T1, T2, T3>(),
            registrar =>
            {
                registrar.Register<T1>();
                registrar.Register<T2>();
                registrar.Register<T3>();
            });
    }

    public static Archetype Create<T1, T2, T3, T4>() where T1 : struct where T2 : struct where T3 : struct where T4 : struct
    {
        return new Archetype(manager => manager.GetEntities<T1, T2, T3, T4>(),
            registrar =>
            {
                registrar.Register<T1>();
                registrar.Register<T2>();
                registrar.Register<T3>();
                registrar.Register<T4>();
            });
    }

    public static Archetype Create<T1, T2, T3, T4, T5>() where T1 : struct where T2 : struct where T3 : struct where T4 : struct where T5 : struct
    {
        return new Archetype(manager => manager.GetEntities<T1, T2, T3, T4, T5>(),
            registrar =>
            {
                registrar.Register<T1>();
                registrar.Register<T2>();
                registrar.Register<T3>();
                registrar.Register<T4>();
                registrar.Register<T5>();
            });
    }

    public static Archetype Create<T1, T2, T3, T4, T5, T6>() where T1 : struct where T2 : struct where T3 : struct where T4 : struct where T5 : struct where T6 : struct
    {
        return new Archetype(manager => manager.GetEntities<T1, T2, T3, T4, T5, T6>(),
            registrar =>
            {
                registrar.Register<T1>();
                registrar.Register<T2>();
                registrar.Register<T3>();
                registrar.Register<T4>();
                registrar.Register<T5>();
                registrar.Register<T6>();
            });
    }
}
