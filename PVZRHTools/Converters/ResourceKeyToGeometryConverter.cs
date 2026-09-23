using System;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace PVZRHTools.Converters;

public class ResourceKeyToGeometryConverter : IValueConverter
{
    public static readonly ResourceKeyToGeometryConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string key || Application.Current is null)
            return null;

        if (Application.Current.TryGetResource(key, Application.Current.ActualThemeVariant, out var resource)
            && resource is Geometry geometry)
            return geometry;

        return null;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
