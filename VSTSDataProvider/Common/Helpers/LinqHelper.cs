using System;
using System.Collections.Generic;
using System.Linq;

namespace VSTSDataProvider.Common;

public static class LinqHelper
{
    public static TSource FirstOrDefaultSafe<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate = null)
    {
        if (source == null) return default;
        if (predicate == null) return source.FirstOrDefault();
        var result = source.FirstOrDefault(predicate);
        return result == null ? default : result;
    }
    public static TSource FirstOrFallback<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate, TSource fallbackValue = default)
    {
        if (source == null) return fallbackValue;
        var result = source.FirstOrDefaultSafe(predicate);
        return result == null ? fallbackValue : result;
    }
}