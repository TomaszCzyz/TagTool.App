using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;

namespace TagTool.App.ViewModels.UserControls;

public partial class FilePreviewerViewModel : ViewModelBase, IDisposable
{
    [ObservableProperty]
    private Bitmap? _current;

    [ObservableProperty]
    private double _viewboxWidth;

    [ObservableProperty]
    private double _viewboxHeight;

    public FilePreviewerViewModel()
    {
        _current = new Bitmap(@"C:\Users\tczyz\MyFiles\localCopyOfDatabase.png");
        ViewboxWidth = _current?.Size.Width ?? 0;
        ViewboxHeight = _current?.Size.Height ?? 0;
    }

    public void Dispose()
    {
        _current?.Dispose();
        GC.SuppressFinalize(this);
    }
}
