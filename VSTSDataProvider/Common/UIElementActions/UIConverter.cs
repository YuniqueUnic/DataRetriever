using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace VSTSDataProvider.Common;

internal class UIConverter
{

}

public class FontSizeScaleConverter : IValueConverter
{
    //    if( value is not double fontSize ) return null;
    //    if( parameter is double scaleTimes ) return fontSize * scaleTimes;
    //    return fontSize * 1.5;
    public object Convert(object value , Type targetType , object parameter , CultureInfo culture)
    {
        double? fontSize = value as double? ?? (double.TryParse(value?.ToString() , out double parsedValue) ? parsedValue : default(double?));

        if( fontSize == null ) return null;

        switch( parameter )
        {
            case string scaleString:
                if( double.TryParse(scaleString , out double scaleTimes) )
                {
                    return fontSize * scaleTimes;
                }
                break;

            case double scaleTimes_1:
                return fontSize * scaleTimes_1;

            default:
                return fontSize * 1.5;
        }

        return null;
    }

    public object ConvertBack(object value , Type targetType , object parameter , CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}

/// <summary>
/// 将 double 类型的值按比例缩放。
/// </summary>
public class DoubleValueScaleConverter : IValueConverter
{
    /// <summary>
    /// 将 double 类型的值按比例缩放。
    /// </summary>
    /// <param name="value">要缩放的值。</param>
    /// <param name="targetType">缩放后的类型。</param>
    /// <param name="parameter">缩放比例。可以是 double 类型的值或表示 double 值的字符串。</param>
    /// <param name="culture">转换器的区域性。</param>
    /// <returns>缩放后的值。</returns>
    public object Convert(object value , Type targetType , object parameter , CultureInfo culture)
    {
        // 将参数 value 强制转换为 double? 类型，如果转换失败，则将其设置为 null。
        double? fontSize = value as double? ?? (double.TryParse(value?.ToString() , out double parsedValue) ? parsedValue : default(double?));

        if( fontSize == null )
        {
            return null!;
        }

        switch( parameter )
        {
            // 如果 parameter 是表示 double 值的字符串，则将其解析为 double 类型并缩放 fontSize 的值。
            case string scaleString:
                if( double.TryParse(scaleString , out double scaleTimes) )
                {
                    return fontSize * scaleTimes;
                }
                break;
            case double scaleTimes_1:
                return fontSize * scaleTimes_1;
            // 如果 parameter 为空或不是上述两种类型，则将 fontSize 的值乘以 1.5 缩放。
            default:
                return fontSize * 1.5;
        }

        return null!;
    }

    public object ConvertBack(object value , Type targetType , object parameter , CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}

[MarkupExtensionReturnType(typeof(IValueConverter))]
public class BoolenNullableToVisibility : MarkupExtension, IValueConverter
{
    public bool Reverse { get; set; }
    public bool IsEnabled { get; set; }

    public BoolenNullableToVisibility( )
    {
        Reverse = false;
        IsEnabled = true;
    }

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        return this;
    }

    /// <summary>
    /// 将 bool? 转换为 Visibility 值。
    /// </summary>
    /// <param name="value">要转换的值。</param>
    /// <param name="targetType">转换后的类型。</param>
    /// <param name="parameter">转换器的参数。</param>
    /// <param name="culture">转换器的区域性。</param>
    /// <returns>转换后的值。</returns>
    public object Convert(object value , Type targetType , object parameter , CultureInfo culture)
    {
        // 根据 IsEnabled 属性返回适当的 Visibility 值。
        if( !IsEnabled )
        {
            return Visibility.Hidden;
        }

        // 将参数 value 强制转换为 bool? 类型。
        bool? isVisible = (bool?)value;

        // 根据 Reverse 属性反转布尔值。
        if( isVisible.HasValue && Reverse )
        {
            isVisible = !isVisible;
        }

        // 如果 isVisible 为 true，则返回 Visibility.Visible，否则返回 Visibility.Collapsed。
        return isVisible.HasValue && isVisible.Value ? Visibility.Visible : Visibility.Collapsed;
    }


    /// <summary>
    /// 将 Visibility 的三种状态（Visible、Collapsed 和 Hidden）转换为 bool? 类型的值。
    /// </summary>
    /// <param name="value">要转换的值。</param>
    /// <param name="targetType">转换后的类型。</param>
    /// <param name="parameter">转换器的参数。</param>
    /// <param name="culture">转换器的区域性。</param>
    /// <returns>转换后的值。</returns>
    public object ConvertBack(object value , Type targetType , object parameter , CultureInfo culture)
    {
        // 将参数 value 强制转换为 Visibility 类型。
        Visibility visibility = (Visibility)value;

        // 如果 IsEnabled 为 false，返回 null。
        if( !IsEnabled )
        {
            return null;
        }

        // 根据 Reverse 属性反转布尔值。
        switch( visibility )
        {
            case Visibility.Visible:
                return Reverse ? (bool?)false : true;
            case Visibility.Hidden:
                return null;
            case Visibility.Collapsed:
                return Reverse ? (bool?)true : false;
            default:
                throw new NotSupportedException($"Invalid visibility value {value}.");
        }
    }
}