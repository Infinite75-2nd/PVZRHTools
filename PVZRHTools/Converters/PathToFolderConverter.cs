using System;
using System.Globalization;
using System.IO;
using Avalonia.Data.Converters;

namespace PVZRHTools.Converters;

public class PathToFolderConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string path || string.IsNullOrWhiteSpace(path)) return "Unnamed";
        var trimmedPath = path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        var lastComponent = Path.GetFileName(trimmedPath);
        return string.Equals(trimmedPath, Path.GetPathRoot(trimmedPath), System.StringComparison.OrdinalIgnoreCase)
            ? trimmedPath
            : // 或根据需求返回 trimmedPath 本身
            lastComponent;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}