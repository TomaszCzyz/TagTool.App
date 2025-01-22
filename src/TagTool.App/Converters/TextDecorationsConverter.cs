using System.Diagnostics;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using TagTool.App.Models;

namespace TagTool.App.Converters;

public class TextDecorationsConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not QuerySegmentState state)
        {
            return new TextDecorationCollection();
        }

        return state switch
        {
            QuerySegmentState.Include => [],
            QuerySegmentState.Exclude =>
            [
                new TextDecoration
                {
                    Location = TextDecorationLocation.Strikethrough,
                    StrokeThickness = 3,
                    StrokeThicknessUnit = TextDecorationUnit.Pixel
                }
            ],
            QuerySegmentState.MustBePresent => new TextDecorationCollection
            {
                new()
                {
                    Location = TextDecorationLocation.Underline,
                    StrokeThickness = 2,
                    StrokeThicknessUnit = TextDecorationUnit.Pixel
                }
            },
            _ => throw new UnreachableException()
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotSupportedException();
}
