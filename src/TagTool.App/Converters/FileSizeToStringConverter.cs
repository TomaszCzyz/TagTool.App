using System.Globalization;
using Avalonia.Data.Converters;
using TagTool.App.Extensions;

namespace TagTool.App.Converters;

public class FileSizeToStringConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is long l
            ? l.GetBytesReadable()
            : value;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotImplementedException();
}
