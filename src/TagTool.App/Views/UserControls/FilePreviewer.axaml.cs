using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using TagTool.App.ViewModels.UserControls;

namespace TagTool.App.Views.UserControls;

public partial class FilePreviewer : UserControl
{
    private FilePreviewerViewModel ViewModel => (FilePreviewerViewModel)DataContext!;

    public FilePreviewer()
    {
        InitializeComponent();
    }

    private void PreviewImage_OnPointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        if (e.KeyModifiers != KeyModifiers.Control)
        {
            e.Handled = false;
            return;
        }

        e.Handled = true;
        var delta = e.Delta.Y * 35;

        if (ViewModel.ViewboxWidth + delta < 50 || ViewModel.ViewboxHeight + delta < 50)
        {
            return;
        }

        ViewModel.ViewboxWidth += delta;
        ViewModel.ViewboxHeight += delta;

        ScrollViewer.Offset += (delta > 0 ? 1 : -1) * new Vector(delta / 2, delta / 2);
    }
}
