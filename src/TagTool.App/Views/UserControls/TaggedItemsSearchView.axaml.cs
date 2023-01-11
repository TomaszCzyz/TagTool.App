using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Markup.Xaml;
using TagTool.App.Models;
using TagTool.App.ViewModels.UserControls;
using TagTool.App.Views.Dialogs;

namespace TagTool.App.Views.UserControls;

public partial class TaggedItemsSearchView : UserControl
{
    private TaggedItemsSearchViewModel ViewModel => (TaggedItemsSearchViewModel)DataContext!;

    public TaggedItemsSearchView()
    {
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
                ViewModel.RemoveLastCommand.Execute(e);
                break;
            case Key.Enter when ViewModel.SelectedItemFromPopup is not null:
                ViewModel.AddTagCommand.Execute(e);
                SearchHelperPopup.IsOpen = false;
                e.Handled = true;
                break;
            case Key.Enter when ViewModel.SelectedItemFromPopup is null:
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
                ViewModel.UpdateSearchCommand.Execute(e);
                break;
        }
    }

    private void InputElement_OnDoubleTapped(object? sender, TappedEventArgs e)
    {
        ViewModel.AddTagCommand.Execute(e);
    }

    private async void SpecialTagInputElement_OnDoubleTapped(object? sender, TappedEventArgs e)
    {
        var dialog = new NameSpecialTagDialog();

        var result = await dialog.ShowDialog<NameSpecialTag?>((Window)VisualRoot!);

        ViewModel.AddSpecialTagCommand.Execute(result);
    }
}
