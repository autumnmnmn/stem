using Utilities.DefaultDictionary;
using System;
using System.Collections.Generic;

namespace Stem.Internal;

internal class AspectRegistry : IAspectRegistrar
{
    private int __nextBit = 0;

    private int __sparsePoolSize = 256;

    private static int __registryCount = 0;

    private readonly int __registryId = __registryCount++;

    private readonly Dictionary<int, Action<int>> __deletionMethodByAspectId = new();

    private readonly Dictionary<int, Action<int>> __sparsePoolSizeUpdaters = new();

    public int SparsePoolSize
    {
        get => __sparsePoolSize;
        set
        {
            __sparsePoolSize = value;
            foreach (var updater in __sparsePoolSizeUpdaters.Values)
            {
                updater(__sparsePoolSize);
            }
        }
    }

    private int NextId
    {
        get
        {
            var output = __nextBit;
            if (++__nextBit == 32)
            {
                throw new Exception("Attempted to register too many aspects.");
            }
            return output;
        }
    }

    public int[] GetSparsePool<T>() where T : struct
    {
        return Registrations<T>.SparsePoolByRegistry[__registryId];
    }

    // Performance bottleneck
    // TODO: finish implementing dense aspects for more performant lookup
    public ref T GetAspectRef<T>(int entityId) where T : struct
    {
        return ref GetPool<T>()[GetSparsePool<T>()[entityId]];
    }

    public T GetAspect<T>(int entityId) where T : struct
    {
        return GetPool<T>()[GetSparsePool<T>()[entityId]];
    }

    public void DeleteAspect<T>(int entityId) where T : struct
    {
        Registrations<T>.DeletedEntriesByRegistry[__registryId].Enqueue(GetSparsePool<T>()[entityId]);
    }

    /// <summary>
    ///     Avoid this, use <see cref="DeleteAspect{T}(int)" /> whenever possible./>
    /// </summary>
    public void DeleteAspect(int entityId, int aspectId)
    {
        __deletionMethodByAspectId[aspectId].Invoke(entityId);
    }

    public void CreateAspect<T>(int entityId) where T : struct
    {
        var pool = Registrations<T>.PoolByRegistry[__registryId];
        if (Registrations<T>.DeletedEntriesByRegistry[__registryId].TryDequeue(out int index))
        {
            pool[index] = new T();
            Registrations<T>.SparsePoolByRegistry[__registryId][entityId] = index;
            return;
        }
        index = Registrations<T>.PoolEntriesByRegistry[__registryId];
        if (pool.Length == Registrations<T>.PoolEntriesByRegistry[__registryId])
        {
            Array.Resize(ref pool, pool.Length * 2);
            Registrations<T>.PoolByRegistry[__registryId] = pool;
        }
        pool[index] = new T();
        ++Registrations<T>.PoolEntriesByRegistry[__registryId];
        Registrations<T>.SparsePoolByRegistry[__registryId][entityId] = index;
        return;
    }

    public ref T CreateAspectRef<T>(int entityId) where T : struct
    {
        var pool = Registrations<T>.PoolByRegistry[__registryId];
        if (Registrations<T>.DeletedEntriesByRegistry[__registryId].TryDequeue(out int index))
        {
            pool[index] = new T();
            Registrations<T>.SparsePoolByRegistry[__registryId][entityId] = index;
            return ref pool[index];
        }
        index = Registrations<T>.PoolEntriesByRegistry[__registryId];
        if (pool.Length == Registrations<T>.PoolEntriesByRegistry[__registryId])
        {
            Array.Resize(ref pool, pool.Length * 2);
            Registrations<T>.PoolByRegistry[__registryId] = pool;
        }
        pool[index] = new T();
        ++Registrations<T>.PoolEntriesByRegistry[__registryId];
        Registrations<T>.SparsePoolByRegistry[__registryId][entityId] = index;
        return ref pool[index];
    }

    public void InsertAspect<T>(int entityId, T aspect) where T : struct
    {
        var pool = Registrations<T>.PoolByRegistry[__registryId];
        var index = Registrations<T>.PoolEntriesByRegistry[__registryId];
        if (pool.Length == Registrations<T>.PoolEntriesByRegistry[__registryId])
        {
            Array.Resize(ref pool, pool.Length * 2);
            Registrations<T>.PoolByRegistry[__registryId] = pool;
        }
        pool[index] = aspect;
        ++Registrations<T>.PoolEntriesByRegistry[__registryId];
        Registrations<T>.SparsePoolByRegistry[__registryId][entityId] = index;
    }

    public T[] GetPool<T>() where T : struct
    {
        return Registrations<T>.PoolByRegistry[__registryId];
    }

    public int GetId<T>() where T : struct
    {
#if DEBUG
        var id = Registrations<T>.IdByRegistry[__registryId];
        if (id.HasValue)
        {
            return id.Value;
        }
        throw new Exception(); // TODO: make an exception type for this
#endif
#pragma warning disable CS0162 // Unreachable code
        return Registrations<T>.IdByRegistry[__registryId].Value;
#pragma warning restore CS0162
    }

    public void Register<T>(AspectType aspectType = AspectType.Sparse) where T : struct
    {
        switch (aspectType)
        {
            case AspectType.Sparse:
                RegisterSparse<T>(false);
                break;

            case AspectType.Dense:
                RegisterDense<T>(false);
                break;

            default:
                throw new NotImplementedException();
        };
    }

    private void RegisterDense<T>(bool buffered) where T : struct
    {
        if (buffered) throw new NotImplementedException();
        if (Registrations<T>.PoolByRegistry[__registryId] is not null)
        {
            return; // Already registered
        }
        Registrations<T>.PoolByRegistry[__registryId] = new T[256];
        Registrations<T>.IdByRegistry[__registryId] = NextId;
        Registrations<T>.IsDenseByRegistry[__registryId] = true;
    }

    private void RegisterSparse<T>(bool buffered) where T : struct
    {
        if (buffered) throw new NotImplementedException();
        if (Registrations<T>.PoolByRegistry[__registryId] is not null)
        {
            return; // Already registered
        }
        Registrations<T>.PoolByRegistry[__registryId] = new T[256];
        Registrations<T>.SparsePoolByRegistry[__registryId] = new int[__sparsePoolSize];
        Registrations<T>.IdByRegistry[__registryId] = NextId;
        Registrations<T>.PoolEntriesByRegistry[__registryId] = 1; // Not true!
        Registrations<T>.DeletedEntriesByRegistry[__registryId] = new();
        Registrations<T>.IsDenseByRegistry[__registryId] = false;
        __deletionMethodByAspectId[Registrations<T>.IdByRegistry[__registryId].Value] = entityId => DeleteAspect<T>(entityId);
        __sparsePoolSizeUpdaters[Registrations<T>.IdByRegistry[__registryId].Value] = newSize =>
        {
            var pool = Registrations<T>.SparsePoolByRegistry[__registryId];
            Array.Resize(ref pool, newSize);
            Registrations<T>.SparsePoolByRegistry[__registryId] = pool;
        };
    }

    private static class Registrations<T> where T : struct
    { // Better not run more than 16 stem instances.
        public static DefaultDictionary<int, int?> IdByRegistry = new((int?) null);

        public static DefaultDictionary<int, int[]> SparsePoolByRegistry = new((int[]) null);
        public static DefaultDictionary<int, T[]> PoolByRegistry = new((T[]) null);
        public static DefaultDictionary<int, int> PoolEntriesByRegistry = new();
        public static DefaultDictionary<int, bool> IsDenseByRegistry = new();
        public static DefaultDictionary<int, Queue<int>> DeletedEntriesByRegistry = new();
    }
}

public enum AspectType
{
    Dense,
    Sparse,
    DenseBuffered,
    SparseBuffered
}
