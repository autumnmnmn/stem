using System;
using System.Collections.Generic;
using System.Linq;

namespace Utilities.Extensions;

public static class Ints
{
    public static IEnumerable<int> UpThrough(this int lower, int upper)
    {
        if (lower > upper) throw new ArgumentOutOfRangeException(nameof(upper));
        for (int value = lower; value <= upper; value++)
        {
            yield return value;
        }
    }

    public static IEnumerable<int> UpUntil(this int lower, int upper) => lower == upper ? Enumerable.Empty<int>() : lower.UpThrough(upper - 1);

    public static IEnumerable<int> DownThrough(this int upper, int lower)
    {
        if (lower > upper) throw new ArgumentOutOfRangeException(nameof(lower));
        for (int value = upper; value >= lower; --value)
        {
            yield return value;
        }
    }

    public static IEnumerable<int> DownUntil(this int upper, int lower) => lower == upper ? Enumerable.Empty<int>() : upper.DownThrough(lower + 1);

    public static IEnumerable<int> Through(this int valueA, int valueB)
        => valueA < valueB ? valueA.UpThrough(valueB) : valueA.DownThrough(valueB);

    public static IEnumerable<int> Until(this int valueA, int valueB)
        => valueA < valueB ? valueA.UpUntil(valueB) : valueA.DownUntil(valueB);

    public static IEnumerable<int> ZeroUpThrough(int value) => 0.UpThrough(value);

    public static IEnumerable<int> ZeroUpUntil(int value) => 0.UpUntil(value);

    public static IEnumerable<int> ZeroThrough(int value) => 0.Through(value);

    public static IEnumerable<int> ZeroUntil(int value) => 0.Until(value);

    public static IEnumerable<int> ZeroDownThrough(int value) => 0.DownThrough(value);

    public static IEnumerable<int> ZeroDownUntil(int value) => 0.DownUntil(value);

    public static IEnumerable<int> OneUpThrough(int value) => 1.UpThrough(value);

    public static IEnumerable<int> OneUpUntil(int value) => 1.UpUntil(value);

    public static IEnumerable<int> OneThrough(int value) => 1.Through(value);

    public static IEnumerable<int> OneUntil(int value) => 1.Until(value);

    public static IEnumerable<int> OneDownThrough(int value) => 1.DownThrough(value);

    public static IEnumerable<int> OneDownUntil(int value) => 1.DownUntil(value);
}
