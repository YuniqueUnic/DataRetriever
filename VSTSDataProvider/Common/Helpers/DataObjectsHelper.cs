namespace VSTSDataProvider.Common.Helpers;

public static class DataObjectsHelper
{
    public static bool IsDefault<T>(this T Tobject , bool isNullNeedException = false)
    {
        if( Tobject is null && !isNullNeedException ) return true;

        return Tobject.Equals(default(T));
    }

    public static bool IsZero(this int value)
    {
        return value.Equals(default);
    }

    public static bool IsNullOrWhiteSpaceOrEmpty(this string value)
    {
        return string.IsNullOrWhiteSpace(value);
    }
}
