using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using TagTool.App.ViewModels.UserControls;

namespace TagTool.App.Views.UserControls;

public partial class ItemsTaggingTabView : UserControl
{
    private ItemsTaggingTabViewModel ViewModel => (ItemsTaggingTabViewModel)DataContext!;

    public ItemsTaggingTabView()
    {
        InitializeComponent();
        ItemsListBox.AddHandler(DragDrop.DropEvent, Drop);
        // DragDropInfoAreaBorder.AddHandler(DragDrop.DragOverEvent, DragOver);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        var window = (Window)VisualRoot!;
        // window.AddHandler(DragDrop.DragEnterEvent, (_, _) => DragDropInfoAreaBorder.IsVisible = true);
        // window.AddHandler(DragDrop.DragLeaveEvent, (_, _) => DragDropInfoAreaBorder.IsVisible = false);
        // window.AddHandler(DragDrop.DropEvent, (_, _) => DragDropInfoAreaBorder.IsVisible = false);

        base.OnApplyTemplate(e);
    }

    private void Drop(object? sender, DragEventArgs e)
    {
        if (!e.Data.Contains(DataFormats.FileNames) || e.Data.GetFileNames() is not { } fullFileNames) return;

        var fileSystemInfos = fullFileNames
            .Select(path => (FileSystemInfo)(Directory.Exists(path)
                ? new DirectoryInfo(path)
                : new FileInfo(path)));

        ViewModel.AddItemsToListCommand.Execute(fileSystemInfos);
    }

    // private void DragOver(object? sender, DragEventArgs e)
    // {
    //     // Only allow Copy or Link as Drop Operations.
    //     e.DragEffects &= (DragDropEffects.Copy | DragDropEffects.Link);
    //
    //     // Only allow if the dragged data contains text or filenames.
    //     if (!e.Data.Contains(DataFormats.Text) && !e.Data.Contains(DataFormats.FileNames))
    //     {
    //         e.DragEffects = DragDropEffects.None;
    //     }
    // }
}
