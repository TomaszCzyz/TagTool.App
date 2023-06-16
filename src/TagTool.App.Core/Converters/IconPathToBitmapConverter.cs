using System.Drawing.Imaging;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using TagTool.App.Core.Services;

namespace TagTool.App.Core.Converters;

public class IconPathToBitmapConverter : IValueConverter
{
    private const string DefaultFileIconAssetUri = "avares://TagTool.App/Assets/Images/round_description_black_36dp.png";
    private const string DefaultFolderIconAssetUri = "avares://TagTool.App/Assets/Images/windows_folder_icon.png";

    private static readonly Stream _defaultFileIcon = AssetLoader.Open(new Uri(DefaultFileIconAssetUri));
    private static readonly Stream _defaultFolderIcon = AssetLoader.Open(new Uri(DefaultFolderIconAssetUri));

    private readonly IFileIconProvider _defaultFileIconProvider = new DefaultFileIconProvider();

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
