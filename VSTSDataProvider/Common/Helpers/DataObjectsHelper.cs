using System;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace VSTSDataProvider.Common.Helpers;

public static class DataObjectsHelper
{
    public static bool IsDefault<T>(this T Tobject , bool isNullNeedException = false)
    {
        if( Tobject is null && !isNullNeedException ) return true;

        return Tobject.Equals(default(T));
    }

    public static bool IsZero<T>(this T value) where T : struct, System.IEquatable<T>
    {
        return value.Equals(default(T));
    }

    public static bool IsNullOrWhiteSpaceOrEmpty(this string value)
    {
        return string.IsNullOrWhiteSpace(value);
    }
}
