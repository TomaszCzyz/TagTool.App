using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.VisualTree;

namespace TagTool.App.Core.Views;

public partial class TaggableItemsSearchBarView : UserControl
{
    private AutoCompleteBox? _autoCompleteBox;
    private TextBox? _textBox;

    private TaggableItemsSearchBarViewModel ViewModel => (TaggableItemsSearchBarViewModel)DataContext!;

    public TaggableItemsSearchBarView()
    {
        InitializeComponent();
    }

    protected override void OnGotFocus(GotFocusEventArgs e)
    {
        base.OnGotFocus(e);
        _textBox?.Focus();
        e.Handled = true;
    }

    private void SearchBarBorder_OnPointerPressed(object? sender, PointerPressedEventArgs e) => _autoCompleteBox?.Focus();

    private void AutoCompleteBox_OnAttachedToVisualTree(object? sender, LogicalTreeAttachmentEventArgs e)
    {
        _autoCompleteBox = (AutoCompleteBox)sender!;

        _autoCompleteBox.AsyncPopulator = ViewModel.GetTagsAsync;
        _autoCompleteBox.ItemFilter = ViewModel.FilterAlreadyUsedTags;
        _autoCompleteBox.AddHandler(KeyDownEvent, AutoCompleteBoxOnKeyDown, RoutingStrategies.Tunnel | RoutingStrategies.Bubble);
    }

    private void AutoCompleteBox_OnLoaded(object? sender, RoutedEventArgs e)
    {
        _textBox = _autoCompleteBox.FindDescendantOfType<TextBox>()!;
        _textBox.AddHandler(PointerPressedEvent, (_, _) => _autoCompleteBox!.IsDropDownOpen = true, handledEventsToo: true);

        _autoCompleteBox!.AddHandler(
            PointerPressedEvent,
            (_, _) =>
            {
                if (_autoCompleteBox.SelectedItem is null)
                {
                    return;
                }

                AddTagToQuery();
            },
            handledEventsToo: true);

        var listBoxItem = _autoCompleteBox.FindAncestorOfType<ListBoxItem>()!;
        listBoxItem.GotFocus += (_, _) => _textBox.Focus();
    }

    private void AutoCompleteBoxOnKeyDown(object? sender, KeyEventArgs e)
    {
        var autoCompleteBox = (AutoCompleteBox)sender!;

        e.Handled = true;

        switch (e.Key)
        {
            case Key.Enter when autoCompleteBox.SelectedItem is not null:
                AddTagToQuery();
                break;
            case Key.Left when _textBox?.CaretIndex == 0 && ViewModel.QuerySegments.Count != 0:
                TagsListBox.Focus();
                TagsListBox.Selection.SelectedItem = ViewModel.QuerySegments.Last();
                break;
            default:
                e.Handled = false;
                break;
        }
    }

    private void AddTagToQuery()
    {
        ViewModel.AddTagToSearchQueryCommand.Execute(_autoCompleteBox!.SelectedItem);

        // workaround for clearing Text in AutoCompleteBox when IsTextCompletionEnabled is true
        _autoCompleteBox.FindDescendantOfType<TextBox>()!.Text = "";
    }

    private void TagsListBox_OnKeyDown(object? sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Delete when ViewModel.QuerySegments.Count != 0:
                if (TagsListBox.SelectedItem is null)
                {
                    return;
                }

                var selectedIndex = TagsListBox.SelectedIndex;

                ViewModel.RemoveTagFromSearchQueryCommand.Execute(TagsListBox.SelectedItem);

                if (selectedIndex >= ViewModel.QuerySegments.Count)
                {
                    selectedIndex = ViewModel.QuerySegments.Count;
                }

                TagsListBox.Selection.Select(selectedIndex);
                TagsListBox.ContainerFromIndex(selectedIndex)?.Focus(NavigationMethod.Directional);

                e.Handled = true;
                break;
            default:
                e.Handled = false;
                break;
        }
    }

    private void TagDisplayText_OnAttachedToLogicalTree(object? sender, RoutedEventArgs routedEventArgs)
    {
        var control = (sender as Control)!;
        var listBoxItem = control.FindAncestorOfType<ListBoxItem>()!;

        listBoxItem.AddHandler(
            KeyDownEvent,
            (o, args) =>
            {
                if (args.Key == Key.Apps)
                {
                    control.ContextFlyout?.ShowAt(listBoxItem);
                }
            },
            RoutingStrategies.Bubble);
    }

    private void PopupFlyoutBase_OnClosing(object? sender, EventArgs eventArgs) => _textBox?.Focus();

    private void InputElement_OnGotFocus(object? sender, GotFocusEventArgs e)
    {
        // Debug.WriteLine(sender);
        // if (TagsListBox.Selection.SelectedItem is not null)
        // {
        //     TagsListBox.ContainerFromItem(TagsListBox.Selection.SelectedItem)?.Focus();
        // }
        // // if (TagsListBox.SelectedItem is null)
        // // {
        //     _textBox?.Focus();
        // // }
    }
}
