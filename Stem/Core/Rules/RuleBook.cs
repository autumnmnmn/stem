using Stem.Internal;
using System;
using System.Collections.Generic;

namespace Stem.Rules;

public class RuleBook : IRuleBook
{
    private readonly RuleRegistry __registry;

    private readonly State __state;

    private Func<IState, bool> __rulebookApplicabilityChecker;

    IEnumerable<IRule> IRuleBook.Rules => (IEnumerable<IRule>)__registry.Rules;

    public RuleBook()
    {
        __registry = new();
        __state = new();
        __rulebookApplicabilityChecker = (_) => true;
    }

    public IRuleBook WithRules(Action<IRuleRegistrar> registerRules)
    {
        registerRules(__registry);
        return this;
    }

    public IRuleBook ApplicableWhen(Func<IState, bool> globalStateCondition)
    {
        __rulebookApplicabilityChecker = globalStateCondition;
        return this;
    }

    bool IRuleBook.IsApplicable(IState globalState)
        => __rulebookApplicabilityChecker(globalState);

    void IRuleBook.Setup(IEntityStore store, IState globalState)
    {
        foreach (var rule in __registry.Rules)
        {
            rule.Setup(store, globalState, __state);
        }
    }
}
