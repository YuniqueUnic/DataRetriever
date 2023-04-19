using System;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace VSTSDataProvider.Common;

[AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
public sealed class StringValueAttribute : Attribute
{
    public string Value { get; }

    public StringValueAttribute([CallerMemberName] string caller = null)
    {
        Value = caller;
    }
}

public static class EnumExtensions
{
    public static string GetStringValue(this Enum value)
    {
        var field = value.GetType().GetField(value.ToString());
        var attribute = field.GetCustomAttribute<StringValueAttribute>();
        return attribute != null ? attribute.Value : value.ToString();
    }

    public static T SetEnumValue<T>(this string value) where T : Enum
    {
        return (T)Enum.Parse(typeof(T), value, true);
    }

    public static T SetEnumValueIgnoreCase<T>(this string value) where T : struct, Enum
    {
        if (!Enum.TryParse(value, true, out T result))
        {
            result = Enum.TryParse("unknown", true, out T unknown) ? unknown : default(T);
        }
        return result;
    }

}
