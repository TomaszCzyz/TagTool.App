using System.Globalization;
using Avalonia.Data.Converters;

namespace TagTool.App.Core.Converters;

public class FontSizeConverter : IValueConverter
{
    public double Ratio { get; set; }

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not double originalFontSize)
        {
            return value;
        }

        return originalFontSize * Ratio;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotImplementedException();
}
