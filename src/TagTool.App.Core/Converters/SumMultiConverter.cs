using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace TagTool.App.Core.Converters;

/// <example>
/// <code>
/// <Line
///     VerticalAlignment="Top"
///     Margin="-1 1 0 0"
///     Stroke="Red"
///     
///     StrokeThickness="1">
///         <Line.StartPoint>
///             <MultiBinding Converter="{StaticResource MovePointXConverter}" ConverterParameter="+">
///                 <Binding ElementName="PART_MainBorder" Path="Bounds.TopLeft" />
///                 <Binding ElementName="PART_MainBorder" Path="CornerRadius.TopLeft" />
///             </MultiBinding>
///         </Line.StartPoint>
///         <Line.EndPoint>
///             <MultiBinding Converter="{StaticResource MovePointXConverter}" ConverterParameter="-">
///                 <Binding ElementName="PART_MainBorder" Path="Bounds.TopRight" />
///                 <Binding ElementName="PART_MainBorder" Path="CornerRadius.TopLeft" />
///             </MultiBinding>
///         </Line.EndPoint>
///     </Line>
/// </code>
/// </example>
public class MovePointXConverter : IMultiValueConverter
{
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count != 2) return null;
        if (values[0] is not Point point || values[1] is not double length) return null;

        parameter ??= "+";
        return (string)parameter == "+"
            ? new Point(point.X + length, point.Y)
            : new Point(point.X - length, point.Y);
    }
}

public class SolidColorBrushToColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not SolidColorBrush brush) return null;

        return brush.Color;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
