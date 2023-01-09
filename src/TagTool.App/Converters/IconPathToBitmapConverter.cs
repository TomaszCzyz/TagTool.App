using System.Drawing.Imaging;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Microsoft.Extensions.DependencyInjection;
using TagTool.App.Core.Services;

namespace TagTool.App.Converters;

public class IconPathToBitmapConverter : IValueConverter
{
    private readonly IFileIconProvider _defaultFileIconProvider = App.Current.Services.GetRequiredService<IFileIconProvider>();
    private static readonly IAssetLoader _assets = AvaloniaLocator.Current.GetRequiredService<IAssetLoader>();

    private static readonly Stream _defaultFileIcon = _assets.Open(new Uri("avares://TagTool.App/Assets/Images/round_description_black_36dp.png"));
    private static readonly Stream _defaultFolderIcon = _assets.Open(new Uri("avares://TagTool.App/Assets/Images/windows_folder_icon.png"));

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string path) return null;

        var length = parameter switch
        {
            double x => (int)x,
            string s => int.TryParse(s, out var v) ? v : default,
            _ => default
        };

        if (Directory.Exists(path))
        {
            return CreateBitmap(_defaultFolderIcon, length);
        }

#pragma warning disable CA1416
        // todo: make this behaviour platform agnostic

        var bitmap = _defaultFileIconProvider.GetFileIcon(path)?.ToBitmap();

        if (bitmap is null)
        {
            return CreateBitmap(_defaultFileIcon, length);
        }

        using var memoryStream = new MemoryStream();
        bitmap.Save(memoryStream, ImageFormat.Png);
        memoryStream.Flush();
        memoryStream.Seek(0, SeekOrigin.Begin);

        return CreateBitmap(memoryStream, length);
#pragma warning restore CA1416
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }

    private static Bitmap CreateBitmap(Stream stream, int length)
    {
        if (length == default)
        {
            return new Bitmap(stream);
        }

        var decodeToWidth = Bitmap.DecodeToWidth(stream, length);
        stream.Seek(0, SeekOrigin.Begin);

        return decodeToWidth;
    }
}
