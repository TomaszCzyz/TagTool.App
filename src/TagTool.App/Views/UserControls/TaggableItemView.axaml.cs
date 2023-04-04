using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using TagTool.App.Core.Models;
using TagTool.App.ViewModels.UserControls;
using TagTool.App.Views.Dialogs;

namespace TagTool.App.Views.UserControls;

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
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
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
        var tagName = (string)e.Data.Get("draggedTag")!;

        ViewModel.TagItCommand.Execute(tagName);
    }

    private async void TagItMenuItem_OnClick(object? sender, RoutedEventArgs e)
    {
        var dialog = new TagFileDialog(ViewModel.Location);
        var _ = await dialog.ShowDialog<(string FileName, Tag[] Tags)>((Window)VisualRoot!);
    }

    private void UntagMenuItem_OnClick(object? sender, RoutedEventArgs e)
    {
        var menuItem = (MenuItem)sender!;
        var textBlock = menuItem.FindLogicalAncestorOfType<TextBlock>()!;
        var tagName = textBlock.Text!;

        ViewModel.UntagItemCommand.Execute(tagName);
    }
}
