using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Markup.Xaml;
using Avalonia.VisualTree;
using Microsoft.Extensions.DependencyInjection;
using TagTool.App.Core.Models;
using TagTool.App.ViewModels.UserControls;
using TagTool.App.Views.Dialogs;

namespace TagTool.App.Views.UserControls;

public partial class FileSystemView : UserControl
{
    private readonly FileSystemViewModel _vm = App.Current.Services.GetRequiredService<FileSystemViewModel>();

    public FileSystemView()
    {
        DataContext = _vm;
        InitializeComponent();

        DataGrid.AddHandler(
            KeyDownEvent,
            DataGrid_OnKeyDown, //todo: split this logic to two handlers (one for quick search scenario, one for navigation scenario)
            handledEventsToo: true);
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void AddressTextBox_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        _vm.CancelAddressChangeCommand.Execute(e);
    }

    private void Border_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var border = (Border)sender!;

        if (e.GetCurrentPoint(border).Properties.PointerUpdateKind != PointerUpdateKind.LeftButtonPressed) return;

        _vm.IsEditing = true;
        AddressTextBox?.Focus();
        AddressTextBox?.SelectAll();
    }

    private void DataGrid_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        TextBlockSelectedItems.Text = $"{DataGrid.SelectedItems.Count} selected |";
    }

    private void Visual_OnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        var dataGridCell = e.Parent.FindAncestorOfType<DataGridCell>()!;

        dataGridCell.AddHandler(DoubleTappedEvent, (_, _) => _vm.NavigateCommand.Execute(null));
        dataGridCell.AddHandler(DragDrop.DragOverEvent, DragOver);
        dataGridCell.AddHandler(DragDrop.DropEvent, Drop);
    }

    private void DragOver(object? sender, DragEventArgs e)
    {
        // Only allow Copy or Link as Drop Operations.
        e.DragEffects &= DragDropEffects.Link;

        // Only allow if the dragged data contains tag
        if (!e.Data.Contains("draggedTag"))
        {
            e.DragEffects = DragDropEffects.None;
        }
    }

    private void Drop(object? sender, DragEventArgs e)
    {
        var dataGridCell = (DataGridCell)sender!;

        var entry = (FileSystemEntry)dataGridCell.DataContext!;
        var tagName = (string)e.Data.Get("draggedTag")!;

        _vm.TagItCommand.Execute((tagName, entry));
        _vm.UpdateTags();
    }

    private void DataGrid_OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (KeyHelpers.IsDigitOrLetter(e.Key))
        {
            _vm.QuickSearchText += e.Key.ToString().ToLower(CultureInfo.CurrentCulture);
            return;
        }

        if (string.IsNullOrEmpty(_vm.QuickSearchText))
        {
            if (e.Key != Key.Back) return;

            _vm.NavigateUpCommand.Execute(null);
            e.Handled = true;

            return;
        }

        switch (e.Key)
        {
            case Key.Back:
                _vm.QuickSearchText = _vm.QuickSearchText[..^1];
                break;
            case Key.Down:
                if (_vm.GoToNextMatchedItemCommand.CanExecute(null))
                {
                    _vm.GoToNextMatchedItemCommand.Execute(null);
                }

                break;
            case Key.Up:
                if (_vm.GoToPreviousMatchedItemCommand.CanExecute(null))
                {
                    _vm.GoToPreviousMatchedItemCommand.Execute(null);
                }

                break;
        }
    }

    private void DataGrid_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        _vm.QuickSearchText = "";
    }

    private async void TagItMenuItem_OnClick(object? sender, RoutedEventArgs e)
    {
        if (_vm.SelectedItem is null) return;

        var dialog = new TagFileDialog(_vm.SelectedItem.DisplayName);
        var _ = await dialog.ShowDialog<(string FileName, Tag[] Tags)>(GetWindow());
    }

    private Window GetWindow() => (Window)VisualRoot!;

    private void TagMenuItem_OnClick(object? sender, RoutedEventArgs e)
    {
        var menuItem = (MenuItem)sender!;
        var textBlock = menuItem.FindLogicalAncestorOfType<TextBlock>()!;
        var dataGridCell = menuItem.FindLogicalAncestorOfType<DataGridCell>()!;

        var tagName = textBlock.Text!;
        var entry = (FileSystemEntry)dataGridCell.DataContext!;

        _vm.UntagItemCommand.Execute((tagName, entry));
        _vm.UpdateTags();
    }

    private void TagsVisibilityToggleButton_OnClick(object? sender, RoutedEventArgs e)
    {
        QuickSearchTextBlock.Text = QuickSearchTextBlock.Text?.Length == 0 ? "t" : "";
    }
}

public static class KeyHelpers
{
    public static bool IsDigitOrLetter(Key key)
    {
        switch (key)
        {
            case Key.D0:
            case Key.D1:
            case Key.D2:
            case Key.D3:
            case Key.D4:
            case Key.D5:
            case Key.D6:
            case Key.D7:
            case Key.D8:
            case Key.D9:
            case Key.A:
            case Key.B:
            case Key.C:
            case Key.D:
            case Key.E:
            case Key.F:
            case Key.G:
            case Key.H:
            case Key.I:
            case Key.J:
            case Key.K:
            case Key.L:
            case Key.M:
            case Key.N:
            case Key.O:
            case Key.P:
            case Key.Q:
            case Key.R:
            case Key.S:
            case Key.T:
            case Key.U:
            case Key.V:
            case Key.W:
            case Key.X:
            case Key.Y:
            case Key.Z:
            case Key.NumPad0:
            case Key.NumPad1:
            case Key.NumPad2:
            case Key.NumPad3:
            case Key.NumPad4:
            case Key.NumPad5:
            case Key.NumPad6:
            case Key.NumPad7:
            case Key.NumPad8:
            case Key.NumPad9:
                return true;
            default:
                return false;
        }
    }
}
