using System.Globalization;
using Avalonia.Data.Converters;
using TagTool.App.Core.Extensions;

namespace TagTool.App.Core.Converters;

public class FileSizeToStringConverter : IValueConverter
{
    private const string Format = "0.##";

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is long l
            ? l.GetBytesReadable(Format)
            : value;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotImplementedException();
}
