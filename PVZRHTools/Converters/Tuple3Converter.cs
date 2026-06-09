using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data.Converters;

namespace PVZRHTools.Converters;

public class Tuple3Converter : IMultiValueConverter
{
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count >= 3)
        {
            try
            {
                // 安全的类型转换
                int wave = values[0] is int w ? w : int.Parse(values[0]!.ToString()!);
                int index = values[1] is int i ? i : int.Parse(values[1]!.ToString()!);
                int type = values[2] is int t ? t : int.Parse(values[2]!.ToString()!);

                return (wave, index, type);
            }
            catch
            {
                return null;
            }
        }

        return null;
    }
}