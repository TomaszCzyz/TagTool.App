using System.Drawing.Imaging;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using TagTool.App.Core.Services;

namespace TagTool.App.Converters;

public class IconToBitmapConverter : IValueConverter
{
    private readonly DefaultFileIconProvider _defaultFileIconProvider = new();

    private static readonly Bitmap _defaultFileIcon
        = new(@"C:\Users\tczyz\Source\Repos\My\TagTool\TagTool.App\TagTool.App\Assets/Images/round_description_black_36dp.png");

    private static readonly Bitmap _defaultFolderIcon
        = new(@"C:\Users\tczyz\Source\Repos\My\TagTool\TagTool.App\TagTool.App\Assets/Images/windows_folder_icon.png");

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string path) return null;

        if (Directory.Exists(path))
        {
            return _defaultFolderIcon;
        }

#pragma warning disable CA1416
        var bitmap = _defaultFileIconProvider.GetFileIcon(path)?.ToBitmap();
        if (bitmap is null)
        {
            return _defaultFileIcon;
        }

        using var memoryStream = new MemoryStream();
        bitmap.Save(memoryStream, ImageFormat.Png);
        memoryStream.Flush();
        memoryStream.Seek(0, SeekOrigin.Begin);

        return parameter is string s && int.TryParse(s, out var length)
            ? Bitmap.DecodeToWidth(memoryStream, length)
            : new Bitmap(memoryStream);
#pragma warning restore CA1416
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
