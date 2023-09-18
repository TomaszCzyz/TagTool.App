using System.Drawing.Imaging;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using TagTool.App.Core.Models;
using TaskExtensions = TagTool.App.Core.Extensions.TaskExtensions;

namespace TagTool.App.Core.Services.Previewers;

public interface IUnsupportedFilePreviewer : IPreviewer
{
    public UnsupportedFilePreviewData? Preview { get; }
}

public partial class UnsupportedFilePreviewData : ObservableObject
{
    [ObservableProperty]
    private Bitmap? _iconPreview;

    [ObservableProperty]
    private string? _fileName;

    [ObservableProperty]
    private string? _fileType;

    [ObservableProperty]
    private string? _fileSize;

    [ObservableProperty]
    private string? _dateModified;
}

public partial class UnsupportedFilePreviewer : ObservableObject, IUnsupportedFilePreviewer, IDisposable
{
    [ObservableProperty]
    private UnsupportedFilePreviewData _preview = new();

    [ObservableProperty]
    private PreviewState _state;

    public bool IsPreviewLoaded => Preview.IconPreview != null;

    public TaggableItem? Item { get; set; }

    private Task<bool>? _iconPreviewTask;
    private Task<bool>? _displayInfoTask;
    private readonly IFileIconProvider _defaultFileIconProvider = new DefaultFileIconProvider();

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    public async Task LoadPreviewAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        State = PreviewState.Loading;

        _iconPreviewTask = LoadIconPreviewAsync(cancellationToken);
        // DisplayInfoTask = LoadDisplayInfoAsync(cancellationToken);

        // await Task.WhenAll(IconPreviewTask, DisplayInfoTask);
        await Task.WhenAll(_iconPreviewTask);

        State = HasFailedLoadingPreview() ? PreviewState.Error : PreviewState.Loaded;
    }

    public async Task<bool> LoadIconPreviewAsync(CancellationToken cancellationToken)
    {
        var path = Item switch
        {
            TaggableFile file => file.Path,
            TaggableFolder folder => folder.Path,
            _ => ""
        };
        var isIconValid = false;

        var isTaskSuccessful = await TaskExtensions.RunSafe(async () =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            await Dispatcher.UIThread.InvokeAsync(async () =>
            {
                cancellationToken.ThrowIfCancellationRequested();

                var bitmap = _defaultFileIconProvider.GetFileIcon(path)?.ToBitmap();

                isIconValid = bitmap != null;

                using var memoryStream = new MemoryStream();
                bitmap?.Save(memoryStream, ImageFormat.Png);
                await memoryStream.FlushAsync(cancellationToken);
                memoryStream.Seek(0, SeekOrigin.Begin);

                // todo: handle default icons
                Preview.IconPreview = new Bitmap(memoryStream); // ?? new SvgImageSource(new Uri("ms-appx:///Assets/Peek/DefaultFileIcon.svg"));
            });
        });

        return isIconValid && isTaskSuccessful;
    }

    // public async Task<bool> LoadDisplayInfoAsync(CancellationToken cancellationToken)
    // {
    //     bool isDisplayValid = false;
    //
    //     var isTaskSuccessful = await TaskExtensions.RunSafe(async () =>
    //     {
    //         // File Properties
    //         cancellationToken.ThrowIfCancellationRequested();
    //
    //         var bytes = await Task.Run(Item.GetSizeInBytes);
    //
    //         cancellationToken.ThrowIfCancellationRequested();
    //
    //         var type = await Task.Run(Item.GetContentTypeAsync);
    //
    //         cancellationToken.ThrowIfCancellationRequested();
    //
    //         var readableFileSize = ReadableStringHelper.BytesToReadableString(bytes);
    //
    //         isDisplayValid = type != null;
    //
    //         await Dispatcher.RunOnUiThread(() =>
    //         {
    //             Preview.FileSize = readableFileSize;
    //             Preview.FileType = type;
    //             return Task.CompletedTask;
    //         });
    //     });
    //
    //     return isDisplayValid && isTaskSuccessful;
    // }

    private bool HasFailedLoadingPreview()
    {
        var isLoadingIconPreviewSuccessful = _iconPreviewTask?.Result ?? false;
        var isLoadingDisplayInfoSuccessful = _displayInfoTask?.Result ?? false;

        return !isLoadingIconPreviewSuccessful || !isLoadingDisplayInfoSuccessful;
    }
}
