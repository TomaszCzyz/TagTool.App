using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.VisualTree;
using TagTool.App.Core.Helpers;
using TagTool.App.ViewModels.UserControls;

namespace TagTool.App.Views.UserControls;

public partial class FileSystemView : UserControl
{
    private FileSystemViewModel ViewModel => (FileSystemViewModel)DataContext!;

    public FileSystemView()
    {
        InitializeComponent();

        DataGrid.AddHandler(PointerWheelChangedEvent, DataGrid_OnPointerWheelChanged, RoutingStrategies.Tunnel);
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
        TextBlockSelectedItems.Text = $"{DataGrid.SelectedItems.Count} selected |";
    }

    private void Visual_OnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        var dataGridCell = e.Parent.FindAncestorOfType<DataGridCell>()!;

        dataGridCell.AddHandler(DoubleTappedEvent, Handler);

        void Handler(object? _, TappedEventArgs args)
        {
            ViewModel.NavigateCommand.Execute(null);
            args.Handled = true;
        }
    }

    private void DataGrid_OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (KeyHelpers.IsDigitOrLetter(e.Key))
        {
            ViewModel.QuickSearchText += e.Key.ToString().ToLower(CultureInfo.CurrentCulture);
            return;
        }

        if (string.IsNullOrEmpty(ViewModel.QuickSearchText))
        {
            if (e.Key != Key.Back) return;

            ViewModel.NavigateUpCommand.Execute(null);
            e.Handled = true;

            return;
        }

        switch (e.Key)
        {
            case Key.Back:
                ViewModel.QuickSearchText = ViewModel.QuickSearchText[..^1];
                break;
            case Key.Down:
                if (ViewModel.GoToNextMatchedItemCommand.CanExecute(null))
                {
                    ViewModel.GoToNextMatchedItemCommand.Execute(null);
                }

                break;
            case Key.Up:
                if (ViewModel.GoToPreviousMatchedItemCommand.CanExecute(null))
                {
                    ViewModel.GoToPreviousMatchedItemCommand.Execute(null);
                }

                break;
        }
    }

    private void DataGrid_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        ViewModel.QuickSearchText = "";
    }

    private void DataGrid_OnPointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        if (!e.KeyModifiers.Equals(KeyModifiers.Control)) return;

        if (e.Delta.Y < 0)
        {
            ViewModel.ZoomOutCommand.Execute(null);
        }
        else
        {
            ViewModel.ZoomInCommand.Execute(null);
        }

        e.Handled = true;
    }
}
