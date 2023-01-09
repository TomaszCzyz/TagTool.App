using Avalonia.Data.Converters;

namespace TagTool.App.Converters;

public class FontSizeConverter : IValueConverter
{
    public double Ratio { get; set; }

    public object? Convert(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
    {
        if (value is not double originalFontSize) return value;

        return originalFontSize * Ratio;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
