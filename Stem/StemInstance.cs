using Stem.Internal;
using Stem.Rules;
using Stem.Rules.Rendering;
using OpenTK.Windowing.Desktop;
using System;

namespace Stem;

public class StemInstance : IStemInstance
{
    private readonly AspectRegistry __aspectRegistry;

    private readonly EntityManager __entityManager;

    private readonly RuleCanon __ruleCanon;

    private long __tickCount;

    private readonly State __state;

    public void RunWindow(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings)
    {
        var window = new StemWindow(this, gameWindowSettings, nativeWindowSettings);
        __state.Set(window);

        Setup();
        window.Run();
        Cleanup();

        window.Dispose();
        __state.Clear<StemWindow>();
    }

    /// <summary>
    ///     This method definitely deserves a rename!
    /// </summary>
    public IDisposable Run()
    {
        return new SetupCleanupHandler(Setup, Cleanup);
    }

    private class SetupCleanupHandler : IDisposable
    {
        private readonly Action __cleanup;

        internal SetupCleanupHandler(Action setup, Action cleanup)
        {
            setup();
            __cleanup = cleanup;
        }

        public void Dispose()
        {
            __cleanup();
        }
    }

    public StemInstance()
    {
        __aspectRegistry = new();
        __entityManager = new(__aspectRegistry);
        __ruleCanon = new();
        __tickCount = 0;
        __state = new();
    }

    public void RegisterAspects(Action<IAspectRegistrar> registerAspects) => registerAspects(__aspectRegistry);

    public void RegisterRuleBooks(Action<IRuleBookRegistrar> registerRuleBooks)
    {
        registerRuleBooks(__ruleCanon);

        foreach (var rule in __ruleCanon.Rules)
        {
            rule.RegisterAspects(__aspectRegistry);
        }
    }

    public void ModifyState(Action<IEntityStore, IState> modifyState) => modifyState(__entityManager, __state);

    public void CreateAction(string name, Action<IEntityStore, IState> action) => __state.Set(name, action);

    private void Setup()
    {
        __ruleCanon.Setup(__entityManager, __state);
    }

    public void ExecuteAllRules(double dt)
    {
        ++__tickCount;
        var tickTime = new TickTime { tick = __tickCount, dt = dt };
        __state.Set(tickTime);
        foreach (var rule in __ruleCanon.ApplicableRules(__state))
        {
            rule.OnTick(__entityManager, tickTime);
        }
    }

    public void ExecuteUpdateRules(double dt)
    {
        ++__tickCount;
        __state.Set(new TickTime { tick = __tickCount, dt = dt });
        foreach (var rule in __ruleCanon.ApplicableRules(__state))
        {
            if (rule is IRenderRule)
            {
                continue;
            }
            rule.OnTick(__entityManager, new() { tick = __tickCount, dt = dt });
        }
    }

    void IStemInstance.ExecuteRenderRules(double dt)
    {
        __state.Set(new TickTime { tick = __tickCount, dt = dt });
        foreach (var rule in __ruleCanon.ApplicableRules(__state))
        {
            if (rule is not IRenderRule)
            {
                continue;
            }
            rule.OnTick(__entityManager, new() { tick = __tickCount, dt = dt });
        }
    }

    private void Cleanup()
    {
        foreach (var rule in __ruleCanon.Rules)
        {
            rule.Cleanup();
        }

        // TODO: clear out entries from static dictionaries in aspect registry and state
        // BETTER YET: don't make things static if they shouldn't be static
    }
}
