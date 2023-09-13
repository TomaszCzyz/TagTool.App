using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.VisualTree;
using TagTool.App.Core.ViewModels;
using TagTool.App.ViewModels.UserControls;
using TagTool.App.Views.Dialogs;

namespace TagTool.App.Views.UserControls;

public partial class FileSystemView : UserControl
{
    private bool _isYesNoDialogOpened;
    private FileSystemViewModel ViewModel => (FileSystemViewModel)DataContext!;

    public FileSystemView()
    {
        InitializeComponent();

        // todo: split this logic to two handlers (one for quick search scenario, one for navigation scenario)
        FolderContentListBox.AddHandler(KeyDownEvent, FolderContent_OnKeyDown, handledEventsToo: true);
    }

    private void AddressTextBox_OnLostFocus(object? sender, RoutedEventArgs e) => ViewModel.CancelAddressChangeCommand.Execute(e);

    private void Border_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var border = (Border)sender!;

        if (e.GetCurrentPoint(border).Properties.PointerUpdateKind != PointerUpdateKind.LeftButtonPressed)
        {
            return;
        }

        ViewModel.IsEditing = true;
        AddressTextBox?.Focus();
        AddressTextBox?.SelectAll();
    }

    private void FolderContent_OnSelectionChanged(object? _, SelectionChangedEventArgs e)
    {
        FolderContentListBox.Focus();
        TextBlockSelectedItems.Text = $"{FolderContentListBox.SelectedItems?.Count ?? 0} selected |";
    }

    private void FolderContent_OnKeyDown(object? sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Back when string.IsNullOrEmpty(FuzzySearchTextBlock.Text):
                ViewModel.NavigateUpCommand.Execute(null);
                FolderContentListBox.FindLogicalDescendantOfType<ListBoxItem>()?.Focus();
                e.Handled = true;
                break;
            case Key.Enter:
                ViewModel.NavigateCommand.Execute(null);
                FolderContentListBox.FindLogicalDescendantOfType<ListBoxItem>()?.Focus();
                e.Handled = true;
                break;
        }
    }

    private void Visual_OnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        var listBoxItem = e.Parent.FindAncestorOfType<ListBoxItem>()!;

        listBoxItem.FocusAdorner = null;
        listBoxItem.AddHandler(DoubleTappedEvent, Handler);
        listBoxItem.AddHandler(KeyDownEvent, OnKeyDown_DeleteHandler, RoutingStrategies.Tunnel);
        return;

        void Handler(object? _, TappedEventArgs args)
        {
            ViewModel.NavigateCommand.Execute(null);
            args.Handled = true;

            FolderContentListBox.FindLogicalDescendantOfType<ListBoxItem>()?.Focus();
        }
    }

    private async Task OnKeyDown_DeleteHandler(object? sender, KeyEventArgs args)
    {
        if (args.Key != Key.Delete)
        {
            return;
        }


        var listBoxItem = (ListBoxItem)sender!;
        var vm = (TaggableItemViewModel)listBoxItem.DataContext!;

        if (!_isYesNoDialogOpened)
        {
            _isYesNoDialogOpened = true;

            var dialog = new YesNoDialog { Question = $"You sure want to delete file {vm.TaggableItem.DisplayName}?" };
            var (answer, _) = await dialog.ShowDialog<(bool Answer, bool Remember)>((Window)VisualRoot!);

            if (answer)
            {
                ViewModel.DeleteTaggableItemCommand.Execute(vm);
            }

            _isYesNoDialogOpened = false;
            args.Handled = true;
        }
    }

    private void AvaloniaObject_OnPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == ItemsControl.ItemCountProperty && FolderContentListBox.Scroll is not null)
        {
            FolderContentListBox.Scroll.Offset = Vector.Zero;
        }
    }
}
