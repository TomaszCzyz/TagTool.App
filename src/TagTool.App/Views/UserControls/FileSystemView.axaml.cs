using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.VisualTree;
using TagTool.App.Core.Extensions;
using TagTool.App.ViewModels.UserControls;

namespace TagTool.App.Views.UserControls;

public partial class FileSystemView : UserControl
{
    private FileSystemViewModel ViewModel => (FileSystemViewModel)DataContext!;

    public FileSystemView()
    {
        InitializeComponent();

        // todo: split this logic to two handlers (one for quick search scenario, one for navigation scenario)
        DataGrid.AddHandler(KeyDownEvent, DataGrid_OnKeyDown, handledEventsToo: true);
    }

    private void AddressTextBox_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        ViewModel.CancelAddressChangeCommand.Execute(e);
    }

    private void Border_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var border = (Border)sender!;

        if (e.GetCurrentPoint(border).Properties.PointerUpdateKind != PointerUpdateKind.LeftButtonPressed) return;

        ViewModel.IsEditing = true;
        AddressTextBox?.Focus();
        AddressTextBox?.SelectAll();
    }

    private void DataGrid_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        TextBlockSelectedItems.Text = $"{DataGrid.SelectedItems?.Count ?? 0} selected |";
    }

    private void DataGrid_OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key.IsDigitOrLetter())
        {
            ViewModel.QuickSearchText += e.Key.ToString().ToLower(CultureInfo.CurrentCulture);
            e.Handled = true;

            return;
        }

        switch (e.Key)
        {
            case Key.Escape when !string.IsNullOrEmpty(ViewModel.QuickSearchText):
                ViewModel.QuickSearchText = "";
                break;
            case Key.Down when !string.IsNullOrEmpty(ViewModel.QuickSearchText):
                if (ViewModel.GoToNextMatchedItemCommand.CanExecute(null))
                {
                    ViewModel.GoToNextMatchedItemCommand.Execute(null);
                }

                break;
            case Key.Up when !string.IsNullOrEmpty(ViewModel.QuickSearchText):
                if (ViewModel.GoToPreviousMatchedItemCommand.CanExecute(null))
                {
                    ViewModel.GoToPreviousMatchedItemCommand.Execute(null);
                }

                break;
            case Key.Back when string.IsNullOrEmpty(ViewModel.QuickSearchText):
                ViewModel.NavigateUpCommand.Execute(null);
                DataGrid.FindLogicalDescendantOfType<ListBoxItem>()?.Focus();
                break;
            case Key.Back:
                ViewModel.QuickSearchText = ViewModel.QuickSearchText[..^1];
                break;
            case Key.Enter:
                ViewModel.NavigateCommand.Execute(null);
                DataGrid.FindLogicalDescendantOfType<ListBoxItem>()?.Focus();
                break;
        }

        e.Handled = true;
    }

    private void DataGrid_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (DataGrid.IsKeyboardFocusWithin) return;
        ViewModel.QuickSearchText = "";
    }

    private void Visual_OnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        var listBoxItem = e.Parent.FindAncestorOfType<ListBoxItem>()!;

        listBoxItem.FocusAdorner = null;
        listBoxItem.AddHandler(DoubleTappedEvent, Handler);
        return;

        void Handler(object? _, TappedEventArgs args)
        {
            ViewModel.NavigateCommand.Execute(null);
            args.Handled = true;

            DataGrid.FindLogicalDescendantOfType<ListBoxItem>()?.Focus();
        }
    }
}
