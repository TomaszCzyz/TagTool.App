using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace TagTool.App.Core.Converters;

public class BoolToBrushConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not bool b) return Brushes.Blue;

        return b ? Brushes.White : Brushes.Red;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException("Conversion from Brush to Bool is not supported. Binding should be OneWay.");
    }
}
