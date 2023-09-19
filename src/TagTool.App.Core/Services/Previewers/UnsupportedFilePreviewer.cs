using System.Drawing.Imaging;
using System.Globalization;
using System.IO.Enumeration;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using TagTool.App.Core.Extensions;
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
        _displayInfoTask = LoadDisplayInfoAsync(cancellationToken);

        await Task.WhenAll(_iconPreviewTask, _displayInfoTask);

        State = HasFailedLoadingPreview() ? PreviewState.Error : PreviewState.Loaded;
    }

    private async Task<bool> LoadIconPreviewAsync(CancellationToken cancellationToken)
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

    public async Task<bool> LoadDisplayInfoAsync(CancellationToken cancellationToken)
    {
        var isDisplayValid = false;

        var isTaskSuccessful = await TaskExtensions.RunSafe(async () =>
        {
            cancellationToken.ThrowIfCancellationRequested();

            UnsupportedFilePreviewData? unsupportedFilePreviewData;
            switch (Item)
            {
                case TaggableFile file:
                    var fileInfo = new FileInfo(file.Path);
                    unsupportedFilePreviewData = new UnsupportedFilePreviewData
                    {
                        FileSize = fileInfo.Length.GetBytesReadable(),
                        DateModified = fileInfo.LastWriteTime.ToString(CultureInfo.CurrentCulture),
                        FileName = fileInfo.Name.ToUpper(CultureInfo.CurrentCulture),
                        FileType = fileInfo.Extension
                    };
                    break;
                case TaggableFolder folder:
                    var folderInfo = new DirectoryInfo(folder.Path);
                    Dispatcher.UIThread.Post(async () => await CalculateDirSize(folderInfo.FullName, cancellationToken));

                    unsupportedFilePreviewData = new UnsupportedFilePreviewData
                    {
                        DateModified = folderInfo.LastWriteTime.ToString(CultureInfo.CurrentCulture),
                        FileName = folderInfo.Name,
                        FileType = "Folder"
                    };
                    break;
                default:
                    unsupportedFilePreviewData = new UnsupportedFilePreviewData();
                    break;
            }

            cancellationToken.ThrowIfCancellationRequested();

            // var type = await Task.Run(Item.GetContentTypeAsync);
            // isDisplayValid = type != null;

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                Preview = unsupportedFilePreviewData;
                return Task.CompletedTask;
            });
        });

        return isDisplayValid && isTaskSuccessful;
    }

    private async Task CalculateDirSize(string path, CancellationToken cancellationToken)
    {
        if (!Directory.Exists(path))
        {
            return;
        }

        var folderSize = await Task.Run(() => CalculateDirSizeInner(path), cancellationToken);

        // Check if during size calculations the currently previewed item has not changed.
        // I think it cannot happen, because CalculateDirSizeInner would first throw because of a cancellation. 
        if (Preview.FileName == Path.GetFileName(path))
        {
            Preview.FileSize = folderSize.GetBytesReadable();
            OnPropertyChanged(nameof(Preview));
        }
    }

    private static long CalculateDirSizeInner(string path)
    {
        var enumeration = new FileSystemEnumerable<long>(
            directory: path,
            transform: (ref FileSystemEntry entry) => entry.Length,
            options: new EnumerationOptions { RecurseSubdirectories = true, IgnoreInaccessible = true })
        {
            ShouldIncludePredicate = (ref FileSystemEntry entry) => !entry.IsDirectory
        };

        var sum = 0L;
        foreach (var size in enumeration)
        {
            // cancellationToken.ThrowIfCancellationRequested();

            sum += size;
        }

        return sum;
    }

    private bool HasFailedLoadingPreview()
    {
        var isLoadingIconPreviewSuccessful = _iconPreviewTask?.Result ?? false;
        var isLoadingDisplayInfoSuccessful = _displayInfoTask?.Result ?? false;

        return !isLoadingIconPreviewSuccessful || !isLoadingDisplayInfoSuccessful;
    }
}
