using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using TagTool.App.ViewModels.UserControls;

namespace TagTool.App.Views.UserControls;

public partial class FilePreviewer : UserControl
{
    private readonly FilePreviewerViewModel _viewModel = App.Current.Services.GetRequiredService<FilePreviewerViewModel>();

    public FilePreviewer()
    {
        DataContext = _viewModel;
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
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

        if (_viewModel.ViewboxWidth + delta < 50 || _viewModel.ViewboxHeight + delta < 50) return;

        _viewModel.ViewboxWidth += delta;
        _viewModel.ViewboxHeight += delta;

        ScrollViewer.Offset += (delta > 0 ? 1 : -1) * new Vector(delta / 2, delta / 2);
    }
}
