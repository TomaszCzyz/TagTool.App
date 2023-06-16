using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Platform.Storage;
using TagTool.App.Core.Extensions;
using TagTool.App.Models;
using TagTool.App.ViewModels.UserControls;
using TagTool.App.Views.Dialogs;

namespace TagTool.App.Views.UserControls;

public partial class TaggableItemsSearchView : UserControl
{
    private TextBox? _textBox;
    private TaggableItemsSearchViewModel ViewModel => (TaggableItemsSearchViewModel)DataContext!;

    public TaggableItemsSearchView()
    {
        InitializeComponent();

        TopMostGrid.AddHandler(DragDrop.DropEvent, Drop);
        // TopMostGrid.AddHandler(DragDrop.DragOverEvent, DragOver);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        var window = (Window)VisualRoot!;
        window.AddHandler(DragDrop.DragEnterEvent, (_, args) => DragDropInfoAreaBorder.IsVisible = args.Data.Contains(DataFormats.Files));
        window.AddHandler(DragDrop.DragLeaveEvent, (_, _) => DragDropInfoAreaBorder.IsVisible = false);
        window.AddHandler(DragDrop.DropEvent, (_, _) => DragDropInfoAreaBorder.IsVisible = false);

        base.OnApplyTemplate(e);
    }

    private async void Drop(object? sender, DragEventArgs e)
    {
        var fileNames = e.Data.GetFiles()?.ToArray() ?? Array.Empty<IStorageItem>();
        if (!e.Data.Contains(DataFormats.Files) || fileNames.Length == 0) return;

        var fileSystemInfos = fileNames
            .Select(item => item.Path.AbsolutePath)
            .Select(path => (FileSystemInfo)(Directory.Exists(path) ? new DirectoryInfo(path) : new FileInfo(path)))
            .ToArray();

        var dialog = new YesNoDialog { Question = "Use internal storage?" };
        var (answer, _) = await dialog.ShowDialog<(bool Answer, bool Remember)>((Window)VisualRoot!);

        var alreadyTaggedItems = await ViewModel.VerifyItemsToAdd(fileSystemInfos);
        var addTagToExisting = false;
        if (alreadyTaggedItems.Count != 0)
        {
            var names = string.Join("\n", alreadyTaggedItems);
            var dialog2 = new YesNoDialog
            {
                Question = $"Following items are already tracked:\n {names}\nDo you want add \"JustAdded\" tag to them too?"
            };
            var result2 = await dialog2.ShowDialog<(bool Answer, bool Remember)>((Window)VisualRoot!);

            addTagToExisting = result2.Answer;
        }

        var toTag = addTagToExisting ? fileSystemInfos : fileSystemInfos.Except(alreadyTaggedItems);

        ViewModel.AddNewItemsCommand.Execute((toTag, answer));
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
            case Key.D1 when e.KeyModifiers.Equals(KeyModifiers.Shift): // exclamation mark
                // todo: change some visuals
                e.Handled = false;
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
        SearchHelperPopup.IsOpen = true;
    }

    private void LogicalOperatorToggleButton_OnClick(object? sender, RoutedEventArgs e)
    {
        var button = (Button)sender!;

        button.Content = button.Content switch
        {
            '|' => '&',
            '&' => '|',
            _ => throw new ArgumentOutOfRangeException(nameof(sender), "unknown operator")
        };
    }
}
