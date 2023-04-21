namespace VSTSDataProvider.Common.Helpers;

public static class StringHelper
{
    public static bool IsNullOrWhiteSpaceOrEmpty(this string value)
    {
        return string.IsNullOrWhiteSpace(value);
    }
}
