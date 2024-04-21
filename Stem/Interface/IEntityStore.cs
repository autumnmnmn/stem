namespace Stem;

public interface IEntityStore
{
    int NewEntity();

    void DeleteEntity(int entityId);

    void Assign<T>(int entityId, T aspect) where T : struct;

    void Assign<T>(int entityId) where T : struct;

    // TODO: void AssignRange<T>(IEnumerable<int> entityIds) where T : struct;

    ref T AssignRef<T>(int entityId) where T : struct;

    bool HasAspect<T>(int entityId) where T : struct;

    void Revoke<T>(int entityId) where T : struct;

    ref T GetAspectRef<T>(int entityId) where T : struct;

    T GetAspect<T>(int entityId) where T : struct;
}
