using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data.Converters;

namespace PVZRHTools.Converters;

public class WaveHeaderConverter : IMultiValueConverter
{
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count < 2 || values[0] is not int key || values[1] is not int currentWave)
            return string.Empty;

        string header = $"第{key}波";

        // 当Key为10的倍数时追加文字"第{Key/10}旗"
        if (key % 10 == 0)
        {
            header += $" 第{key / 10}旗";
        }

        // 当vm中的CurrentWave与Key相等时追加文字"当前波"
        if (key == currentWave)
        {
            header += " 当前波";
        }

        return header;
    }
}