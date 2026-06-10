using System;
using System.Globalization;
using System.IO;
using Avalonia.Data.Converters;

namespace PVZRHTools.Converters;

public class PathToNameConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string path)
        {
            var modPath = Path.GetFileNameWithoutExtension(path);
            return Path.HasExtension(modPath) && Path.GetExtension(modPath) is ".dll"
                ? Path.GetFileNameWithoutExtension(modPath)
                : modPath;
        }

        return null;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}