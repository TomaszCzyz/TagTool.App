using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Media;
using TagTool.App.Core.Models;

namespace TagTool.App.Core.Views;

public partial class TaggableItemView : UserControl
{
    private TaggableItemViewModel ViewModel => (TaggableItemViewModel)DataContext!;

    public TaggableItemView()
    {
        InitializeComponent();

        AddHandler(DragDrop.DragOverEvent, DragOver);
        AddHandler(DragDrop.DropEvent, Drop);
        AddHandler(DragDrop.DragEnterEvent, (_, _) => DragDropInfoAreaBorder.BorderBrush = Brushes.Gray);
        AddHandler(DragDrop.DragLeaveEvent, (_, _) => DragDropInfoAreaBorder.BorderBrush = Brushes.Transparent);
        AddHandler(DragDrop.DropEvent, (_, _) => DragDropInfoAreaBorder.BorderBrush = Brushes.Transparent);

        // Tunnel strategy allows elements higher in hierarchy to handle this event differently, e.g. FileSystemView -> navigate to folder
        // AddHandler(DoubleTappedEvent, (_, _) => ViewModel.ExecuteLinkedActionCommand.Execute(null), RoutingStrategies.Tunnel);

        // todo: I cannot add AddHandler(OnKeyDown,...), because routed event does not go inside ListBoxItem... why?
    }

    private void DragOver(object? sender, DragEventArgs e)
    {
        e.DragEffects &= DragDropEffects.Link;

        if (!e.Data.Contains("draggedTag"))
        {
            e.DragEffects = DragDropEffects.None;
        }
    }

    private void Drop(object? sender, DragEventArgs e)
    {
        var tag = (Tag)e.Data.Get("draggedTag")!;

        // ViewModel.TagItCommand.Execute(tag);
    }

    private void UntagMenuItem_OnClick(object? sender, RoutedEventArgs e)
    {
        var menuItem = (MenuItem)sender!;
        var textBlock = menuItem.FindLogicalAncestorOfType<TextBlock>()!;

        // ViewModel.UntagItemCommand.Execute(textBlock.DataContext);
    }
}
