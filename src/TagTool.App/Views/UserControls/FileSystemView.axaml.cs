using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.VisualTree;
using Microsoft.Extensions.DependencyInjection;
using TagTool.App.Core.Helpers;
using TagTool.App.ViewModels.UserControls;

namespace TagTool.App.Views.UserControls;

public partial class FileSystemView : UserControl
{
    private readonly FileSystemViewModel _viewModel = App.Current.Services.GetRequiredService<FileSystemViewModel>();

    public FileSystemView()
    {
        DataContext = _viewModel;
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
        _viewModel.CancelAddressChangeCommand.Execute(e);
    }

    private void Border_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var border = (Border)sender!;

        if (e.GetCurrentPoint(border).Properties.PointerUpdateKind != PointerUpdateKind.LeftButtonPressed) return;

        _viewModel.IsEditing = true;
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
            _viewModel.NavigateCommand.Execute(null);
            args.Handled = true;
        }
    }

    private void DataGrid_OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (KeyHelpers.IsDigitOrLetter(e.Key))
        {
            _viewModel.QuickSearchText += e.Key.ToString().ToLower(CultureInfo.CurrentCulture);
            return;
        }

        if (string.IsNullOrEmpty(_viewModel.QuickSearchText))
        {
            if (e.Key != Key.Back) return;

            _viewModel.NavigateUpCommand.Execute(null);
            e.Handled = true;

            return;
        }

        switch (e.Key)
        {
            case Key.Back:
                _viewModel.QuickSearchText = _viewModel.QuickSearchText[..^1];
                break;
            case Key.Down:
                if (_viewModel.GoToNextMatchedItemCommand.CanExecute(null))
                {
                    _viewModel.GoToNextMatchedItemCommand.Execute(null);
                }

                break;
            case Key.Up:
                if (_viewModel.GoToPreviousMatchedItemCommand.CanExecute(null))
                {
                    _viewModel.GoToPreviousMatchedItemCommand.Execute(null);
                }

                break;
        }
    }

    private void DataGrid_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        _viewModel.QuickSearchText = "";
    }
}
