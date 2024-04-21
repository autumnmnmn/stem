using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using Utilities.Extensions;

namespace Stem.Internal;

internal class EntityManager : IEntityStore
{
    private EntityMetadata[] __entityArray;
    private readonly Queue<int> __deletedEntities;
    private int __lastEntity = -1;
    private readonly AspectRegistry __aspectRegistry;

    public EntityManager(AspectRegistry aspectRegistry)
    {
        __entityArray = new EntityMetadata[256];
        __aspectRegistry = aspectRegistry;
        __deletedEntities = new();
    }

    public int NewEntity()
    {
        if (__deletedEntities.TryDequeue(out int id))
        {
            __entityArray[id] = new EntityMetadata { id = id };
            return id;
        }
        id = ++__lastEntity;
        if (__entityArray.Length <= id)
        {
            Array.Resize(ref __entityArray, __entityArray.Length * 2);
            __aspectRegistry.SparsePoolSize = __entityArray.Length;
        }
        __entityArray[id] = new EntityMetadata { id = id };
        return id;
    }

    public void DeleteEntity(int entityId)
    {
        __deletedEntities.Enqueue(entityId);
        foreach (var index in __entityArray[entityId].aspectMask.GetIndices())
        {
            __aspectRegistry.DeleteAspect(entityId, index);
        }
        __entityArray[entityId] = new();
    }

    public void Assign<T>(int entityId, T aspect) where T : struct
    {
        var componentId = __aspectRegistry.GetId<T>();
        __entityArray[entityId].aspectMask.Set(componentId);
        __aspectRegistry.InsertAspect(entityId, aspect);
    }

    public void Assign<T>(int entityId) where T : struct
    {
        var componentId = __aspectRegistry.GetId<T>();
        __entityArray[entityId].aspectMask.Set(componentId);
        __aspectRegistry.CreateAspect<T>(entityId);
    }

    public ref T AssignRef<T>(int entityId) where T : struct
    {
        var componentId = __aspectRegistry.GetId<T>();
        __entityArray[entityId].aspectMask.Set(componentId);
        return ref __aspectRegistry.CreateAspectRef<T>(entityId);
    }

    public bool HasAspect<T>(int entityId) where T : struct
    {
        var componentId = __aspectRegistry.GetId<T>();
        return __entityArray[entityId].aspectMask.IsSet(componentId);
    }

    public ref T GetAspectRef<T>(int entityId) where T : struct
    {
        return ref __aspectRegistry.GetAspectRef<T>(entityId);
    }

    public T GetAspect<T>(int entityId) where T : struct
    {
        return __aspectRegistry.GetAspect<T>(entityId);
    }

    public void Revoke<T>(int entityId) where T : struct
    {
        var componentId = __aspectRegistry.GetId<T>();
        __entityArray[entityId].aspectMask.Unset(componentId);
        __aspectRegistry.DeleteAspect<T>(entityId);
    }

    public int[] EntitiesByMask(BitVector32 mask)
    {
        // TODO: Optimize this by checking which aspect in the mask exists on the smallest number of entities, then only check the mask of those entities.
        // This will require tracking the entity associated with each aspect, but that should be a relatively small amount of memory.
        List<int> entities = new();
        for (int i = 0; i < __entityArray.Length; ++i)
        {
            if (__entityArray[i].aspectMask.CheckMask(mask))
            {
                entities.Add(i);
            }
        }
        return entities.ToArray();
    }

    public int[] GetEntities<T1>() where T1 : struct
    {
        return EntitiesByMask(GetMask<T1>());
    }

    public int[] GetEntities<T1, T2>() where T1 : struct where T2 : struct
    {
        return EntitiesByMask(GetMask<T1, T2>());
    }

    public int[] GetEntities<T1, T2, T3>() where T1 : struct where T2 : struct where T3 : struct
    {
        return EntitiesByMask(GetMask<T1, T2, T3>());
    }

    public int[] GetEntities<T1, T2, T3, T4>() where T1 : struct where T2 : struct where T3 : struct where T4 : struct
    {
        return EntitiesByMask(GetMask<T1, T2, T3, T4>());
    }

    public int[] GetEntities<T1, T2, T3, T4, T5>() where T1 : struct where T2 : struct where T3 : struct where T4 : struct where T5 : struct
    {
        return EntitiesByMask(GetMask<T1, T2, T3, T4, T5>());
    }

    public int[] GetEntities<T1, T2, T3, T4, T5, T6>() where T1 : struct where T2 : struct where T3 : struct where T4 : struct where T5 : struct where T6 : struct
    {
        return EntitiesByMask(GetMask<T1, T2, T3, T4, T5, T6>());
    }

    #region GetMask<T1 ... T6>

    private BitVector32 GetMask<T1>() where T1 : struct
    {
        return Bits.FromIndices(__aspectRegistry.GetId<T1>());
    }

    private BitVector32 GetMask<T1, T2>() where T1 : struct where T2 : struct
    {
        return Bits.FromIndices(__aspectRegistry.GetId<T1>(), __aspectRegistry.GetId<T2>());
    }

    private BitVector32 GetMask<T1, T2, T3>() where T1 : struct where T2 : struct where T3 : struct
    {
        return Bits.FromIndices(__aspectRegistry.GetId<T1>(), __aspectRegistry.GetId<T2>(), __aspectRegistry.GetId<T3>());
    }

    private BitVector32 GetMask<T1, T2, T3, T4>() where T1 : struct where T2 : struct where T3 : struct where T4 : struct
    {
        return Bits.FromIndices(__aspectRegistry.GetId<T1>(), __aspectRegistry.GetId<T2>(), __aspectRegistry.GetId<T3>(), __aspectRegistry.GetId<T4>());
    }

    private BitVector32 GetMask<T1, T2, T3, T4, T5>() where T1 : struct where T2 : struct where T3 : struct where T4 : struct where T5 : struct
    {
        return Bits.FromIndices(__aspectRegistry.GetId<T1>(), __aspectRegistry.GetId<T2>(), __aspectRegistry.GetId<T3>(), __aspectRegistry.GetId<T4>(), __aspectRegistry.GetId<T5>());
    }

    private BitVector32 GetMask<T1, T2, T3, T4, T5, T6>() where T1 : struct where T2 : struct where T3 : struct where T4 : struct where T5 : struct where T6 : struct
    {
        return Bits.FromIndices(__aspectRegistry.GetId<T1>(), __aspectRegistry.GetId<T2>(), __aspectRegistry.GetId<T3>(), __aspectRegistry.GetId<T4>(), __aspectRegistry.GetId<T5>(), __aspectRegistry.GetId<T6>());
    }

    #endregion
}
