using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using JetBrains.Annotations;
using TagTool.App.Core.Models;
using TaskExtensions = TagTool.App.Core.Extensions.TaskExtensions;

namespace TagTool.App.Core.Services.Previewers;

[UsedImplicitly]
public partial class RasterImagePreviewer : ObservableObject, IRasterImagePreviewer, IDisposable
{
    private static readonly HashSet<string> _supportedFileTypes = new()
    {
        // Image types
        ".bmp",
        ".gif",
        ".jpg",
        ".jfif",
        ".jfi",
        ".jif",
        ".jpeg",
        ".jpe",
        ".png",
        ".tif", // very slow for large files: no thumbnail?
        ".tiff", // NEED TO TEST
        ".dib", // NEED TO TEST
        ".heic",
        ".heif",
        ".hif", // NEED TO TEST
        ".avif", // NEED TO TEST
        ".jxr",
        ".wdp",
        ".ico", // NEED TO TEST
        ".thumb", // NEED TO TEST
        // Raw types
        ".arw",
        ".cr2",
        ".crw",
        ".erf",
        ".kdc", // NEED TO TEST
        ".mrw",
        ".nef",
        ".nrw",
        ".orf",
        ".pef",
        ".raf",
        ".raw",
        ".rw2",
        ".rwl",
        ".sr2",
        ".srw",
        ".srf",
        ".dcs", // NEED TO TEST
        ".dcr",
        ".drf", // NEED TO TEST
        ".k25",
        ".3fr",
        ".ari", // NEED TO TEST
        ".bay", // NEED TO TEST
        ".cap", // NEED TO TEST
        ".iiq",
        ".eip", // NEED TO TEST
        ".fff",
        ".mef",
        // ".mdc", // Crashes in GetFullBitmapFromPathAsync
        ".mos",
        ".R3D",
        ".rwz", // NEED TO TEST
        ".x3f",
        ".ori",
        ".cr3"
    };

    private Bitmap? _lowQualityThumbnailPreview;
    private Bitmap? _highQualityThumbnailPreview;
    private Task<bool>? _lowQualityThumbnailTask;
    private Task<bool>? _highQualityThumbnailTask;
    private Task<bool>? _fullQualityImageTask;

    [ObservableProperty]
    private Bitmap? _preview;

    [ObservableProperty]
    private Size? _imageSize;

    [ObservableProperty]
    private Size _maxImageSize;

    [ObservableProperty]
    private double _scalingFactor;

    public TaggableFile? Item { get; set; }

    private bool IsHighQualityThumbnailLoaded => _highQualityThumbnailTask?.Status == TaskStatus.RanToCompletion;

    private bool IsFullImageLoaded => _fullQualityImageTask?.Status == TaskStatus.RanToCompletion;

    public void Dispose()
    {
        Clear();
        GC.SuppressFinalize(this);
    }

    public PreviewState State { get; set; }

    public static bool IsFileTypeSupported(string fileExt) => _supportedFileTypes.Contains(fileExt);

    public async Task LoadPreviewAsync(CancellationToken cancellationToken)
    {
        if (Item is null)
        {
            State = PreviewState.Uninitialized;
            return;
        }

        Clear();
        State = PreviewState.Loading;

        _lowQualityThumbnailTask = LoadLowQualityThumbnailAsync(cancellationToken);
        _highQualityThumbnailTask = LoadHighQualityThumbnailAsync(cancellationToken);
        _fullQualityImageTask = LoadFullQualityImageAsync(cancellationToken);

        cancellationToken.ThrowIfCancellationRequested();

        await Task.WhenAll(_lowQualityThumbnailTask, _highQualityThumbnailTask, _fullQualityImageTask);

        // If Preview is still null, FullQualityImage was not available. Preview the thumbnail instead.
        Preview ??= _highQualityThumbnailPreview ?? _lowQualityThumbnailPreview;

        if (Preview == null && HasFailedLoadingPreview())
        {
            State = PreviewState.Error;
        }
    }

    partial void OnPreviewChanged(Bitmap? value)
    {
        if (Preview != null)
        {
            State = PreviewState.Loaded;
        }
    }

    private Task<bool> LoadLowQualityThumbnailAsync(CancellationToken cancellationToken)
        => TaskExtensions.RunSafe(async () =>
        {
            cancellationToken.ThrowIfCancellationRequested();

            await Dispatcher.UIThread.InvokeAsync(async () =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (!IsFullImageLoaded && !IsHighQualityThumbnailLoaded)
                {
                    await using var stream = new FileStream(Item!.Path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
                    _lowQualityThumbnailPreview = Bitmap.DecodeToHeight(stream, 256, BitmapInterpolationMode.LowQuality);
                }
            });
        });

    private Task<bool> LoadHighQualityThumbnailAsync(CancellationToken cancellationToken)
        => TaskExtensions.RunSafe(async () =>
        {
            cancellationToken.ThrowIfCancellationRequested();

            await Dispatcher.UIThread.InvokeAsync(async () =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (!IsFullImageLoaded)
                {
                    await using var stream = new FileStream(Item!.Path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
                    _highQualityThumbnailPreview = Bitmap.DecodeToHeight(stream, 720, BitmapInterpolationMode.MediumQuality);
                }
            });
        });

    private Task<bool> LoadFullQualityImageAsync(CancellationToken cancellationToken)
        => TaskExtensions.RunSafe(async () =>
        {
            cancellationToken.ThrowIfCancellationRequested();

            await Dispatcher.UIThread.InvokeAsync(async () =>
            {
                cancellationToken.ThrowIfCancellationRequested();

                await using var stream = new FileStream(Item!.Path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);

                // todo: we can get max size of a previewer window and also do 'Bitmap.DecodeTo...'
                Preview = new Bitmap(stream);
                OnPropertyChanged(nameof(Preview));
            });
        });

    private bool HasFailedLoadingPreview()
    {
        var hasFailedLoadingLowQualityThumbnail = !(_lowQualityThumbnailTask?.Result ?? true);
        var hasFailedLoadingHighQualityThumbnail = !(_highQualityThumbnailTask?.Result ?? true);
        var hasFailedLoadingFullQualityImage = !(_fullQualityImageTask?.Result ?? true);

        return hasFailedLoadingLowQualityThumbnail && hasFailedLoadingHighQualityThumbnail && hasFailedLoadingFullQualityImage;
    }

    private void Clear()
    {
        _lowQualityThumbnailPreview?.Dispose();
        _highQualityThumbnailPreview?.Dispose();
        Preview?.Dispose();
        _lowQualityThumbnailPreview = null;
        _highQualityThumbnailTask = null;
        Preview = null;
    }
}
