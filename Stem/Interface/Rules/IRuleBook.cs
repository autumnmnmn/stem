using Stem.Internal;
using System;
using System.Collections.Generic;

namespace Stem.Rules;

public interface IRuleBook
{
    internal bool IsApplicable(IState globalState);

    internal IEnumerable<IRule> Rules { get; }

    internal void Setup(IEntityStore store, IState globalState);

    IRuleBook WithRules(Action<IRuleRegistrar> registerRules);

    IRuleBook ApplicableWhen(Func<IState, bool> globalStateCondition);
}

