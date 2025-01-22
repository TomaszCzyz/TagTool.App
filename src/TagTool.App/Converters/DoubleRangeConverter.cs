using System.Globalization;
using Avalonia.Data.Converters;

namespace TagTool.App.Converters;

public class DoubleRangeConverter : IValueConverter
{
    public double InputMin { get; set; }

    public double InputMax { get; set; }

    public double OutputMin { get; set; }

    public double OutputMax { get; set; }

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not double d)
        {
            return null;
        }

        var p = (InputMin - InputMax + d) / InputMin;

        return (OutputMax - OutputMin) * p;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotSupportedException();
}
