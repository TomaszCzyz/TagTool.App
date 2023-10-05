using System.Globalization;
using Avalonia.Data.Converters;
using CronExpressionDescriptor;

namespace TagTool.App.Converters;

public class CronToDescriptionConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string s)
        {
            return null;
        }

        try
        {
            return ExpressionDescriptor.GetDescription(s);
        }
        catch
        {
            return "Incorrect cron.";
        }
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotImplementedException();
}
