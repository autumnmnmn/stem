using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Utilities.Extensions;

public static class Bits
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Set(this ref BitVector32 bits, int bitIndex)
        => bits[1 << bitIndex] = true;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Unset(this ref BitVector32 bits, int bitIndex)
        => bits[1 << bitIndex] = false;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsSet(this ref BitVector32 bits, int bitIndex)
        => bits[1 << bitIndex];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool CheckMask(this ref BitVector32 bits, BitVector32 bitMask)
        => (bits.Data & bitMask.Data) == bitMask.Data;

    public static IEnumerable<int> GetIndices(this BitVector32 bits)
    {
        return Ints.ZeroUpUntil(32).Where(bitIndex => bits.IsSet(bitIndex));
    }

    public static BitVector32 FromIndices(params int[] indices)
    {
        BitVector32 bits = new();
        foreach (var index in indices)
        {
            bits.Set(index);
        }
        return bits;
    }
}
