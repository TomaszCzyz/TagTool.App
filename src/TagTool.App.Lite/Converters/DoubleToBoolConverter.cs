using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace TagTool.App.Lite.Converters;

public class DoubleToBoolConverter : IValueConverter
{
    public double Threshold { get; set; }

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not double d) return null;

        return d >= Threshold;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
