using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using TagTool.App.Extensions;
using TagTool.App.ViewModels.UserControls;

namespace TagTool.App.Views.UserControls;

public partial class FileSystemView : UserControl
{
    private readonly FileSystemViewModel _vm = Application.Current?.CreateInstance<FileSystemViewModel>()!;

    public FileSystemView()
    {
        DataContext = _vm;
        InitializeComponent();
    }

    private void AddressTextBox_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        _vm.CancelNavigationCommand.Execute(e);
    }

    private void Border_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is not InputElement element) return;

        var currentPoint = e.GetCurrentPoint(element);

        if (currentPoint.Properties.PointerUpdateKind != PointerUpdateKind.LeftButtonPressed) return;

        _vm.IsEditing = true;
        AddressTextBox?.Focus();
        AddressTextBox?.SelectAll();
    }

    private void TextBlockDataGridItem_OnDoubleTapped(object? sender, TappedEventArgs tappedEventArgs)
    {
        if (sender is not TextBlock textBlock) return;

        var path = Path.Join(_vm.Address, textBlock.Text);

        if (!Directory.Exists(path)) return;

        var directoryInfo = new DirectoryInfo(path);
        _vm.OpenItem(directoryInfo);
    }
}
