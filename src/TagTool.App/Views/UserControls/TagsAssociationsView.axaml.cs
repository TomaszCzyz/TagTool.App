using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using JetBrains.Annotations;
using TagTool.App.Core.Models;
using TagTool.App.ViewModels.UserControls;

namespace TagTool.App.Views.UserControls;

[UsedImplicitly]
public partial class TagsAssociationsView : UserControl
{
    private TagsAssociationsViewModel ViewModel => (TagsAssociationsViewModel)DataContext!;

    public TagsAssociationsView()
    {
        InitializeComponent();
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
        var tag = (ITag)e.Data.Get("draggedTag")!;

        var groupName = ((AssociationData)((ItemsControl)sender!).DataContext!).GroupName;
        ViewModel.AddTagToSynonymsGroupCommand.Execute((tag, groupName));
    }

    private void TagsInGroupCell_OnLoaded(object? sender, RoutedEventArgs e)
    {
        var itemsControl = (ItemsControl)sender!;
        itemsControl.AddHandler(DragDrop.DragOverEvent, DragOver);
        itemsControl.AddHandler(DragDrop.DropEvent, Drop);
    }

    private void RemoveFromGroupMenuItem_OnClick(object? sender, RoutedEventArgs e)
    {
        var tag = ((MenuItem)sender!).DataContext;

        ViewModel.RemoveTagFromSynonymsCommand.Execute(tag);
    }
}
