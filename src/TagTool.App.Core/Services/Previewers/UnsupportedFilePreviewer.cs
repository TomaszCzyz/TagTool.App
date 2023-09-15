using Avalonia;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using TagTool.App.Core.Models;

namespace TagTool.App.Core.Services.Previewers;

public interface IUnsupportedFilePreviewer : IPreviewer
{
    public UnsupportedFilePreviewData? Preview { get; }
}

public partial class UnsupportedFilePreviewData : ObservableObject
{
    [ObservableProperty]
    private IImage? _iconPreview;

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

    public UnsupportedFilePreviewer(TaggableItem item)
    {
        Item = item;
        if (item is TaggableFile file)
        {
            Preview.FileName = Path.GetFileName(file.Path);
        }
    }

    public bool IsPreviewLoaded => Preview.IconPreview != null;

    private TaggableItem Item { get; }

    private Task<bool>? IconPreviewTask { get; set; }

    private Task<bool>? DisplayInfoTask { get; set; }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    public Task<PreviewSize> GetPreviewSizeAsync(CancellationToken cancellationToken)
    {
        var size = new Size(680, 500);
        var previewSize = new PreviewSize(size, true);
        return Task.FromResult(previewSize);
    }

    public async Task LoadPreviewAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        State = PreviewState.Loading;

        IconPreviewTask = LoadIconPreviewAsync(cancellationToken);
        // DisplayInfoTask = LoadDisplayInfoAsync(cancellationToken);

        // await Task.WhenAll(IconPreviewTask, DisplayInfoTask);
        await Task.WhenAll(IconPreviewTask);

        State = HasFailedLoadingPreview() ? PreviewState.Error : PreviewState.Loaded;
    }

    public async Task<bool> LoadIconPreviewAsync(CancellationToken cancellationToken)
    {
        bool isIconValid = false;

        // var isTaskSuccessful = await TaskExtensions.RunSafe(async () =>
        // {
        //     cancellationToken.ThrowIfCancellationRequested();
        //     await Dispatcher.UIThread.InvokeAsync(async () =>
        //     {
        //         cancellationToken.ThrowIfCancellationRequested();
        //
        //         var iconBitmap = await IconHelper.GetThumbnailAsync(Item.Path, cancellationToken)
        //                          ?? await IconHelper.GetIconAsync(Item.Path, cancellationToken);
        //
        //         cancellationToken.ThrowIfCancellationRequested();
        //
        //         isIconValid = iconBitmap != null;
        //
        //         // Preview.IconPreview = iconBitmap ?? new SvgImageSource(new Uri("ms-appx:///Assets/Peek/DefaultFileIcon.svg"));
        //     });
        // });

        return isIconValid;// && isTaskSuccessful;
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
        var isLoadingIconPreviewSuccessful = IconPreviewTask?.Result ?? false;
        var isLoadingDisplayInfoSuccessful = DisplayInfoTask?.Result ?? false;

        return !isLoadingIconPreviewSuccessful || !isLoadingDisplayInfoSuccessful;
    }
}
