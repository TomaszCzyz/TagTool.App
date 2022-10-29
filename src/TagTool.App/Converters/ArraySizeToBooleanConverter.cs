using System.Globalization;
using Avalonia.Data.Converters;

namespace TagTool.App.Converters;

public class ArraySizeToBooleanConverter : IValueConverter
{
    public int Threshold { get; set; }

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not int count) return null;

        return count > Threshold;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
