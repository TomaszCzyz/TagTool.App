using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using TagTool.App.Services;

namespace TagTool.App.Converters;

public class IconPathToBitmapConverter : IValueConverter
{
    private const string DefaultFileIconAssetUri = "avares://TagTool.App/Assets/Images/outline_description_white_24dp.png";
    private const string DefaultFolderIconAssetUri = "avares://TagTool.App/Assets/Images/outline_folder_white_24dp.png";

    private static readonly Stream _defaultFileIcon = AssetLoader.Open(new Uri(DefaultFileIconAssetUri));
    private static readonly Stream _defaultFolderIcon = AssetLoader.Open(new Uri(DefaultFolderIconAssetUri));

    private readonly DefaultFileIconProvider _fileIconProvider = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string path)
        {
            return null;
        }

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

        var bitmap = _fileIconProvider.GetFileIcon(path, length);

        return bitmap ?? CreateBitmap(_defaultFileIcon, length);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotImplementedException();

    private static Bitmap CreateBitmap(Stream stream, int length)
    {
        stream.Seek(0, SeekOrigin.Begin);
        if (length == 0)
        {
            return new Bitmap(stream);
        }

        var decodeToHeight = Bitmap.DecodeToHeight(stream, length);
        stream.Seek(0, SeekOrigin.Begin);

        return decodeToHeight;
    }
}
