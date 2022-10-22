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

    private void BorderNameCell_OnDoubleTapped(object? sender, TappedEventArgs e)
    {
        if (sender is null) return;

        var border = (Border)sender;
        var grid = (Grid)border.Parent!;
        var textBlock = (TextBlock)grid.Children.First(child => child.GetType() == typeof(TextBlock));

        var directoryInfo = new DirectoryInfo(Path.Join(_vm.Address, textBlock.Text));

        if (directoryInfo.Exists)
        {
            _vm.OpenItem(directoryInfo);
        }
    }
}
