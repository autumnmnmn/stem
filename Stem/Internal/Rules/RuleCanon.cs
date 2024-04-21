using Stem.Rules;
using System.Collections.Generic;
using System.Linq;

namespace Stem.Internal;

internal class RuleCanon : IRuleBookRegistrar
{
    private readonly List<IRuleBook> __ruleBooks;

    internal IEnumerable<IRuleBook> RuleBooks
        => __ruleBooks;

    internal IEnumerable<IRule> Rules
        => RuleBooks.SelectMany(book => book.Rules);

    internal IEnumerable<IRuleBook> ApplicableRuleBooks(IState state)
        => __ruleBooks.Where(book => book.IsApplicable(state));

    internal IEnumerable<IRule> ApplicableRules(IState state)
        => ApplicableRuleBooks(state).SelectMany(book => book.Rules);

    internal RuleCanon()
    {
        __ruleBooks = new();
    }

    TRuleBook IRuleBookRegistrar.AddRuleBook<TRuleBook>(TRuleBook ruleBook)
    {
        __ruleBooks.Add(ruleBook);
        return ruleBook;
    }

    RuleBook IRuleBookRegistrar.AddRuleBook()
    {
        var ruleBook = new RuleBook();
        __ruleBooks.Add(ruleBook);
        return ruleBook;
    }

    internal void Setup(IEntityStore store, State globalState)
    {
        foreach (var ruleBook in RuleBooks)
        {
            ruleBook.Setup(store, globalState);
        }
    }
}
