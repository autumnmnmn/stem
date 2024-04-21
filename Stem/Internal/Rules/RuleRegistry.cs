using Stem.Rules;
using System;
using System.Collections.Generic;

namespace Stem.Internal;

internal class RuleRegistry : IRuleRegistrar
{
    private readonly List<IRule> __rules;

    internal IEnumerable<IRule> Rules => __rules;

    internal RuleRegistry()
    {
        __rules = new();
    }

    void IRuleRegistrar.Register<T>()
    {
        T rule = new();
        __rules.Add(rule);
    }

    void IRuleRegistrar.Register<T>(out T rule)
    {
        rule = new();
        __rules.Add(rule);
    }

    void IRuleRegistrar.Register<T>(Func<T> ruleFactory)
    {
        T rule = ruleFactory();
        __rules.Add(rule);
    }

    void IRuleRegistrar.Register<T>(Func<T> ruleFactory, out T rule)
    {
        rule = ruleFactory();
        __rules.Add(rule);
    }
}
