namespace Stem.Rules;

public interface IRuleBookRegistrar
{
    TRuleBook AddRuleBook<TRuleBook>(TRuleBook ruleBook) where TRuleBook : IRuleBook;

    RuleBook AddRuleBook();
}
