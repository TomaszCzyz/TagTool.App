﻿using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using TagTool.App.Core.Models;

namespace TagTool.App.Core.Converters;

public class LocationAndWidthToIconMultiConverter : IMultiValueConverter
{
    private static readonly IconPathToBitmapConverter _iconPathToBitmapConverter = new();

    public double IconToFontSizeRatio { get; set; } = 1.5;

    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count != 2) return null;
        if (values[0] is not string path || values[1] is not double fontSize) return null;

        return _iconPathToBitmapConverter.Convert(path, typeof(Bitmap), fontSize * IconToFontSizeRatio, culture);
    }
}

public class TaggableItemToToIconConverter : IValueConverter
{
    private static readonly IconPathToBitmapConverter _iconPathToBitmapConverter = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not ITaggableItem taggableItem) return null;

        return taggableItem switch
        {
            TaggableFile file => _iconPathToBitmapConverter.Convert(file.Path, typeof(Bitmap), null, culture),
            TaggableFolder folder => _iconPathToBitmapConverter.Convert(folder.Path, typeof(Bitmap), null, culture),
            _ => null
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotSupportedException();
}
