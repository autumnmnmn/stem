using System;

namespace Stem;

internal interface IStemInstance
{
    void ExecuteUpdateRules(double dt);

    void ExecuteRenderRules(double dt);

    void ModifyState(Action<IEntityStore, IState> modifyState);
}
