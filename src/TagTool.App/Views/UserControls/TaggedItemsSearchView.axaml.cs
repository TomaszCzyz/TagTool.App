using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using TagTool.App.ViewModels.UserControls;

namespace TagTool.App.Views.UserControls;

public partial class TaggedItemsSearchView : UserControl
{
    private readonly TaggedItemsSearchViewModel _viewModel = App.Current.Services.GetRequiredService<TaggedItemsSearchViewModel>();

    public TaggedItemsSearchView()
    {
        DataContext = _viewModel;
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        SearchHelperPopup.IsOpen ^= true;
    }

    private void InputElement_OnGotFocus(object? sender, GotFocusEventArgs e)
    {
        SearchHelperPopup.IsOpen = true;
    }

    private void StyledElement_OnAttachedToLogicalTree(object? sender, LogicalTreeAttachmentEventArgs e)
    {
        var textBox = (TextBox)sender!;

        textBox.AddHandler(KeyDownEvent, OnKeyDown, RoutingStrategies.Tunnel);
        // textBox.AddHandler(KeyDownEvent, EnsurePopupOpen, RoutingStrategies.Tunnel);
        textBox.AddHandler(PointerPressedEvent, EnsurePopupOpen, RoutingStrategies.Tunnel);
    }

    private void EnsurePopupOpen(object? sender, RoutedEventArgs e)
    {
        SearchHelperPopup.IsOpen = true;
    }

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        var textBox = (TextBox)sender!;

        switch (e.Key)
        {
            case Key.Back when string.IsNullOrEmpty(textBox.Text):
                _viewModel.RemoveLastCommand.Execute(e);
                break;
            case Key.Enter when _viewModel.SelectedItemFromPopup is not null:
                _viewModel.AddTagCommand.Execute(e);
                SearchHelperPopup.IsOpen = false;
                e.Handled = true;
                break;
            case Key.Enter when _viewModel.SelectedItemFromPopup is null:
                SearchHelperPopup.IsOpen = false;
                e.Handled = true;
                break;
            case Key.Right:
                SearchResultsListBox.SelectedIndex++;
                e.Handled = true;
                break;
            case Key.Left:
                SearchResultsListBox.SelectedIndex--;
                e.Handled = true;
                break;
            case Key.Down when !SearchHelperPopup.IsOpen: // focus search results in dataGrid
                SearchResultsDataGrid.Focus();
                e.Handled = true;
                break;
            default:
                _viewModel.UpdateSearchCommand.Execute(e);
                break;
        }
    }

    private void InputElement_OnDoubleTapped(object? sender, TappedEventArgs e)
    {
        _viewModel.AddTagCommand.Execute(e);
    }
}
