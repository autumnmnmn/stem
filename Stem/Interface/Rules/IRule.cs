using Stem.Internal;

namespace Stem.Rules;

public interface IRule
{
    internal void Cleanup();

    internal void OnTick(EntityManager manager, TickTime time);

    internal void Setup(IEntityStore store, IState globalState, IState bookState);

    internal void RegisterAspects(IAspectRegistrar aspectRegistrar);
}
