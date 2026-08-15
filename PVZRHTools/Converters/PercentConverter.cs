using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace PVZRHTools.Converters;

/// <summary>
/// 显示值 = 源数据 × 100(取整),写回源数据 = 显示值 ÷ 100。
/// 用于以整数百分比编辑小数源数据(如源 1.0 显示为 100)。
/// </summary>
public class PercentConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value switch
        {
            double d => (int)Math.Round(d * 100.0),
            float f => (int)Math.Round(f * 100.0),
            int i => i * 100,
            _ => value,
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value switch
        {
            int i => i / 100.0,
            double d => d / 100.0,
            float f => f / 100.0,
            _ => 0.0,
        };
    }
}
