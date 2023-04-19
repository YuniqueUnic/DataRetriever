using System;
using System.Globalization;
using System.Windows.Data;

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
