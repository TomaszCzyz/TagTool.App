using System.Globalization;
using Avalonia.Data.Converters;

namespace TagTool.App.Converters;

public class StringToBoolConverter : IValueConverter
{
    public required object StringToCompare { get; set; }

    public bool ValueWhenMatched { get; set; } = true;

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var isMatch = value is not null && (value.ToString()?.Equals(StringToCompare.ToString(), StringComparison.OrdinalIgnoreCase) ?? false);
        return isMatch ? ValueWhenMatched : !ValueWhenMatched;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotImplementedException();
}
