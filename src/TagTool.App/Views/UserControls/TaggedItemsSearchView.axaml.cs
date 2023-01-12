using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Markup.Xaml;
using TagTool.App.Extensions;
using TagTool.App.Models;
using TagTool.App.ViewModels.UserControls;
using TagTool.App.Views.Dialogs;

namespace TagTool.App.Views.UserControls;

public partial class TaggedItemsSearchView : UserControl
{
    private TextBox? _textBox;
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
        _textBox = (TextBox)sender!;

        _textBox.AddHandler(KeyDownEvent, OnKeyDown, RoutingStrategies.Tunnel);
    }

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        var textBox = (TextBox)sender!;

        SearchHelperPopup.IsOpen = true;
        e.Handled = true;

        switch (e.Key)
        {
            case Key.Back when string.IsNullOrEmpty(textBox.Text):
                ViewModel.RemoveLastCommand.Execute(e);
                break;
            case Key.Enter when ViewModel.SelectedItemFromPopup is not null:
                ViewModel.AddTagCommand.Execute(e);
                SearchHelperPopup.IsOpen = false;
                break;
            case Key.Enter when ViewModel.SelectedItemFromPopup is null:
                SearchHelperPopup.IsOpen = false;
                break;
            case Key.Right:
                SelectNextTagInPopup();
                break;
            case Key.Left:
                SelectPreviousTagInPopup();
                break;
            case Key.Down when !SearchHelperPopup.IsOpen:
                SearchResultsDataGrid.Focus();
                break;
            default:
                e.Handled = false;
                break;
        }
    }

    private ListBox[] SelectableListBoxes => new[] { SearchResultsListBox, PopularTagsListBox };

    private void SelectPreviousTagInPopup()
    {
        var selectedListBox = Array.Find(SelectableListBoxes, listBox => listBox.SelectedItem is not null) ?? SearchResultsListBox;

        if (!selectedListBox.IsFirstItemSelected())
        {
            selectedListBox.SelectedIndex--;
            return;
        }

        // reset selection of current listBox and go to the last elem of 'previous' list box
        selectedListBox.SelectedIndex = -1;

        if (ReferenceEquals(selectedListBox, SearchResultsListBox))
        {
            PopularTagsListBox.SelectLast();
            return;
        }

        if (ReferenceEquals(selectedListBox, PopularTagsListBox))
        {
            SearchResultsListBox.SelectLast();
        }
    }

    private void SelectNextTagInPopup()
    {
        var selectedListBox = Array.Find(SelectableListBoxes, listBox => listBox.SelectedItem is not null) ?? SearchResultsListBox;

        if (!selectedListBox.IsLastItemSelected())
        {
            selectedListBox.SelectedIndex++;
            return;
        }

        // reset selection of current listBox and go to the first elem of 'next' list box
        selectedListBox.SelectedIndex = -1;

        if (ReferenceEquals(selectedListBox, SearchResultsListBox))
        {
            PopularTagsListBox.SelectFirst();
            return;
        }

        if (ReferenceEquals(selectedListBox, PopularTagsListBox))
        {
            SearchResultsListBox.SelectFirst();
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

    private void SearchBarInputElement_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        _textBox?.Focus();
    }
}
