using Avalonia.Media.Imaging;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;

namespace TagTool.App.ViewModels.UserControls;

public partial class FilePreviewerViewModel : ViewModelBase, IDisposable
{
    [ObservableProperty]
    private string _noPreviewMessage = "No file selected";

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string? _currentFilePath;

    [ObservableProperty]
    private Bitmap? _current;

    [ObservableProperty]
    private double _viewboxWidth; //todo: move this logic to "code behind" 

    [ObservableProperty]
    private double _viewboxHeight; //todo: move this logic to "code behind"

    partial void OnCurrentFilePathChanged(string? value)
    {
        if (!File.Exists(value))
        {
            NoPreviewMessage = "No file selected";
            return;
        }

        var supportedExtensions = new[] { "JPG", "JPEG", "JPEGXL", "PNG", "RAW", "WEBP" };
        if (!supportedExtensions.Contains(Path.GetExtension(value)[1..], StringComparer.OrdinalIgnoreCase))
        {
            NoPreviewMessage = "The file extension is not supported for preview";
            Current = null;
            return;
        }

        NoPreviewMessage = "";
        Dispatcher.UIThread.Post(LoadPreview);
    }

    private void LoadPreview()
    {
        IsLoading = true;

        Current = new Bitmap(CurrentFilePath!);
        ViewboxWidth = Current?.Size.Width ?? 0;
        ViewboxHeight = Current?.Size.Height ?? 0;

        IsLoading = false;
    }

    public void Dispose()
    {
        _current?.Dispose();
        GC.SuppressFinalize(this);
    }
}
