using System;

namespace Stem.Rules;

public interface IRuleRegistrar
{
    void Register<T>() where T : IRule, new();

    void Register<T>(Func<T> ruleFactory) where T : IRule;

    void Register<T>(out T rule) where T : IRule, new();

    void Register<T>(Func<T> ruleFactory, out T rule) where T : IRule;
}
