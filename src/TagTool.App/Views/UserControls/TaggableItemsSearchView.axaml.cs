using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Platform.Storage;
using TagTool.App.Core.ViewModels;
using TagTool.App.ViewModels.UserControls;
using TagTool.App.Views.Dialogs;

namespace TagTool.App.Views.UserControls;

public partial class TaggableItemsSearchView : UserControl
{
    private TaggableItemsSearchViewModel ViewModel => (TaggableItemsSearchViewModel)DataContext!;

    public TaggableItemsSearchView()
    {
        InitializeComponent();

        DragDropInfoAreaBorder.AddHandler(DragDrop.DropEvent, Drop);
        // DragDropInfoAreaBorder.AddHandler(DragDrop.DragOverEvent, DragOver);

        TaggableItemsListBox.AddHandler(KeyDownEvent, OnKeyDown_ExecuteLinkedAction, handledEventsToo: true);
        TaggableItemsListBox.AddHandler(DoubleTappedEvent, OnDoubleTapped_ExecuteLinkedAction, handledEventsToo: true);
    }

    protected override void OnGotFocus(GotFocusEventArgs e)
    {
        base.OnGotFocus(e);

        SearchBarView.Focus();
        e.Handled = true;
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        var window = (Window)VisualRoot!;
        window.AddHandler(DragDrop.DragEnterEvent, (_, args) => DragDropInfoAreaBorder.IsVisible = args.Data.Contains(DataFormats.Files));
        window.AddHandler(DragDrop.DragLeaveEvent, (_, _) => DragDropInfoAreaBorder.IsVisible = false);
        window.AddHandler(DragDrop.DropEvent, (_, _) => DragDropInfoAreaBorder.IsVisible = false);

        base.OnApplyTemplate(e);
    }

    private static void OnDoubleTapped_ExecuteLinkedAction(object? sender, TappedEventArgs args)
    {
        // todo: if itemType == Folder, then open folder in TagTool.a new App's FileSystemView
        if (sender is ListBox { SelectedItem: TaggableItemViewModel vm })
        {
            vm.ExecuteLinkedActionCommand.Execute(null);
        }
    }

    private static void OnKeyDown_ExecuteLinkedAction(object? sender, KeyEventArgs args)
    {
        // todo: if itemType == Folder, then open folder in TagTool.a new App's FileSystemView
        if (args.Key != Key.Enter)
        {
            return;
        }

        if (sender is ListBox { SelectedItem: TaggableItemViewModel vm })
        {
            vm.ExecuteLinkedActionCommand.Execute(null);
        }
    }

    private async void Drop(object? sender, DragEventArgs e)
    {
        var fileNames = e.Data.GetFiles()?.ToArray() ?? [];
        if (!e.Data.Contains(DataFormats.Files) || fileNames.Length == 0)
        {
            return;
        }

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
}
