using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Runtime.Versioning;
using Avalonia.Platform;
using JetBrains.Annotations;
using TagTool.App.Core.Contracts;
using Bitmap = Avalonia.Media.Imaging.Bitmap;

namespace TagTool.App.Core.TaggableFile;

[UsedImplicitly]
public class TaggableFileIconResolver : ITaggableItemIconResolver<TaggableFile>
{
    private const string DefaultFileIconAssetUri = "avares://TagTool.App.Core/Assets/Images/outline_description_white_24dp.png";

    private static readonly Stream _defaultFileIcon = AssetLoader.Open(new Uri(DefaultFileIconAssetUri));

    public Bitmap GetIcon(TaggableFile item, int? length)
    {
        if (!Path.Exists(item.Path))
        {
            return CreateBitmap(_defaultFileIcon, length);
        }

        if (OperatingSystem.IsWindows())
        {
            return TryGetIcon(item.Path, length, out var result)
                ? result
                : CreateBitmap(_defaultFileIcon, length);
        }

        return CreateBitmap(_defaultFileIcon, length);
    }

    [SupportedOSPlatform("windows")]
    private static bool TryGetIcon(string filePath, int? length, [NotNullWhen(true)] out Bitmap? result)
    {
        if (Icon.ExtractAssociatedIcon(filePath) is not { } icon)
        {
            result = null;
            return false;
        }

        using var memoryStream = new MemoryStream();
        icon.Save(memoryStream);
        memoryStream.Flush();
        memoryStream.Seek(0, SeekOrigin.Begin);

        result = CreateBitmap(memoryStream, length);
        return true;
    }

    private static Bitmap CreateBitmap(Stream stream, int? length)
    {
        stream.Seek(0, SeekOrigin.Begin);
        if (length == 0)
        {
            return new Bitmap(stream);
        }

        return length.HasValue
            ? Bitmap.DecodeToHeight(stream, length.Value)
            : new Bitmap(stream);
    }
}
