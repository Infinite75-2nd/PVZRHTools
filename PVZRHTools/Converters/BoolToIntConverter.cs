using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace PVZRHTools.Converters;

public class BoolToIntConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return boolValue ? 1 : 0;
        }

        return 0;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is int intValue)
        {
            return intValue != 0;
        }

        return false;
    }
}