using System.Diagnostics;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using TagTool.App.Core.Models;

namespace TagTool.App.Core.Converters;

public class TextDecorationsConverter : IMultiValueConverter
{
    public object Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values is not [QuerySegmentState state]) return new TextDecorationCollection();

        return state switch
        {
            QuerySegmentState.Exclude => new TextDecorationCollection
            {
                new() { Location = TextDecorationLocation.Strikethrough, StrokeThickness = 3, StrokeThicknessUnit = TextDecorationUnit.Pixel }
            },
            QuerySegmentState.Include => new TextDecorationCollection(),
            QuerySegmentState.MustBePresent => new TextDecorationCollection
            {
                new() { Location = TextDecorationLocation.Underline, StrokeThickness = 2, StrokeThicknessUnit = TextDecorationUnit.Pixel }
            },
            _ => throw new UnreachableException()
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
