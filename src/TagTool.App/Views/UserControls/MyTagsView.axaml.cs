using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.VisualTree;
using JetBrains.Annotations;
using TagTool.App.Core.Models;
using TagTool.App.ViewModels.UserControls;

namespace TagTool.App.Views.UserControls;

[UsedImplicitly]
public partial class MyTagsView : UserControl
{
    private MyTagsViewModel ViewModel => (MyTagsViewModel)DataContext!;

    public MyTagsView()
    {
        InitializeComponent();
    }

    private void Visual_OnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        var listBoxItem = e.Parent.FindAncestorOfType<ListBoxItem>()!;

        listBoxItem.AddHandler(PointerPressedEvent, HandleDrag, handledEventsToo: true);
    }

    private void HandleDrag(object? sender, PointerPressedEventArgs e)
    {
        if (sender is not ListBoxItem { DataContext: ITag tag }
            || e.GetCurrentPoint(null).Properties.PointerUpdateKind != PointerUpdateKind.LeftButtonPressed)
        {
            return;
        }

        var dragData = new DataObject();
        dragData.Set("draggedTag", tag);

        _ = DragDrop.DoDragDrop(e, dragData, DragDropEffects.Link);
    }
}
