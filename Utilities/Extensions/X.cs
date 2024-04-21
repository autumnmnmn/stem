using System;
using System.Collections.Generic;
using System.Linq;

namespace Utilities.Extensions;

public static class X
{
    public static IEnumerable<T> Repeated<T>(this T @object, uint times) 
    {
        // Should this throw an exception, since Enumerable.Empty should be used for such cases?
        if (times < 1) yield break; 

        for (int index = 0; index < times; ++index) yield return @object;    
    }

    public static IEnumerable<T> Once<T>(this T @object)
    { 
        yield return @object; 
    }

    public static void ForEach<T>(this IEnumerable<T> sequence, Action<T> action)
    {
        foreach (var value in sequence)
        {
            action(value);
        }
    }

    public static IEnumerable<(T First, T Second)> Adjacents<T>(this IReadOnlyList<T> sequence)
    {
        return Ints.OneUpUntil(sequence.Count).Select(index => (sequence[index-1], sequence[index]));
    }

    public static IEnumerable<(T Item, int Index)> Indexed<T>(this IEnumerable<T> sequence)
    {
        return sequence.Select((item, index) => (item, index));
    }
}
