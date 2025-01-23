using System.Globalization;
using Avalonia.Data.Converters;
using TagTool.App.Contracts;

namespace TagTool.App.Converters;

public class ItemOperationsConverter : IMultiValueConverter
{
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count != 2)
        {
            return null;
        }

        if (values[0] is not Dictionary<Type, string[]> operationsMap || values[1] is not TaggableItemBase itemType)
        {
            return null;
        }

        return operationsMap.TryGetValue(itemType.GetType(), out var operations) ? operations : null;
    }
}
