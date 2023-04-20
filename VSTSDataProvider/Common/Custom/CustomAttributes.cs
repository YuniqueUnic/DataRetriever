using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace VSTSDataProvider.Common;

[AttributeUsage(AttributeTargets.Field , Inherited = false , AllowMultiple = false)]
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
    private sealed class EnumInfo<TEnum, TObject>
    {
        public TEnum Enum;
        public TObject EnumValue;
    }

    public static string GetStringValue(this Enum value)
    {
        var field = value.GetType().GetField(value.ToString());
        var attribute = field.GetCustomAttribute<StringValueAttribute>();
        return attribute?.Value ?? value.ToString();
        // return attribute.Value != null ? attribute.Value : value.ToString();
    }

    public static T SetEnumValue<T>(this string value) where T : Enum
    {
        return (T)Enum.Parse(typeof(T) , value , true);
    }

    public static T SetEnumValueIgnoreCase<T>(this string value) where T : struct, Enum
    {
        var enumInfos = new List<EnumInfo<T , string>>();
        var fields = typeof(T).GetFields(BindingFlags.Public | BindingFlags.Static);
        foreach( var field in fields )
        {
            var stringValueAttr = field.GetCustomAttribute<StringValueAttribute>();
            if( stringValueAttr.Value != null )
            {
                enumInfos.Add(new EnumInfo<T , string>()
                {
                    Enum = (T)Enum.Parse(typeof(T) , field.Name , true) ,
                    EnumValue = stringValueAttr.Value ,
                });
            }
            else
            {
                enumInfos.Add(new EnumInfo<T , string>()
                {
                    Enum = (T)Enum.Parse(typeof(T) , field.Name , true) ,
                    EnumValue = field.Name ,
                });
            }
        }
        var enumInfo = enumInfos.Find(ei => ei.EnumValue.Equals(value , StringComparison.OrdinalIgnoreCase));
        return enumInfo?.Enum ?? default;
    }

}