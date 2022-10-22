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
        if (sender is not TextBox { DataContext: FileSystemViewModel fileSystem }) return;

        fileSystem.CancelNavigationCommand.Execute(e);
    }

    private void Border_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is not Border { DataContext: FileSystemViewModel fileSystem } border) return;

        if (e.GetCurrentPoint(border).Properties.PointerUpdateKind != PointerUpdateKind.LeftButtonPressed) return;

        fileSystem.IsEditing = true;
        AddressTextBox?.Focus();
        AddressTextBox?.SelectAll();
    }

    private void DataGrid_OnDoubleTapped(object? sender, TappedEventArgs e)
    {
        if (sender is not DataGrid { DataContext: FileSystemViewModel fileSystem }) return;

        fileSystem.OpenItemCommand.Execute(null);
    }
}
