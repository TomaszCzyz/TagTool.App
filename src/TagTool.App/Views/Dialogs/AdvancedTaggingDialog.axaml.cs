using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Platform.Storage;
using TagTool.App.ViewModels.Dialogs;
using TagTool.App.ViewModels.UserControls;

namespace TagTool.App.Views.Dialogs;

public partial class AdvancedTaggingDialog : Window
{
    private AdvancedTaggingDialogViewModel ViewModel => (AdvancedTaggingDialogViewModel)DataContext!;

    public AdvancedTaggingDialog()
    {
        InitializeComponent();
#if DEBUG
        this.AttachDevTools();
#endif

        AddHandler(DragDrop.DropEvent, Drop);
        AddHandler(DragDrop.DragOverEvent, DragOver);
        AddHandler(DragDrop.DragEnterEvent, (_, _) => DragDropInfoAreaBorder.IsVisible = true);
        AddHandler(DragDrop.DragLeaveEvent, (_, _) => DragDropInfoAreaBorder.IsVisible = false);
        AddHandler(DragDrop.DropEvent, (_, _) => DragDropInfoAreaBorder.IsVisible = false);
    }

    private void DragOver(object? sender, DragEventArgs e)
    {
        // Only allow Copy or Link as Drop Operations.
        e.DragEffects &= (DragDropEffects.Copy | DragDropEffects.Link);

        // Only allow if the dragged data contains text or filenames.
        if (!e.Data.Contains(DataFormats.Text) && !e.Data.Contains(DataFormats.Files))
        {
            e.DragEffects = DragDropEffects.None;
        }
    }

    private void Drop(object? sender, DragEventArgs e)
    {
        if (!e.Data.Contains(DataFormats.Files)) return;
        if (e.Data.GetFiles() is not { } paths) return;

        foreach (var path in paths.Select(item => item.Path.AbsolutePath))
        {
            FileSystemInfo fileSystemInfo = Directory.Exists(path) ? new DirectoryInfo(path) : new FileInfo(path);
            ViewModel.AddItemCommand.Execute(fileSystemInfo);
        }
    }

    private void TreeView_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (!PreviewToggleButton.IsChecked ?? false) return;
        if (TreeView.SelectedItems is not { Count: 1 } selectedItems) return;

        var selectedNode = (AdvancedTaggingDialogViewModel.Node)selectedItems[0]!;
        var previewerViewModel = (FilePreviewerViewModel)FilePreviewer?.DataContext!;

        previewerViewModel.CurrentFilePath = selectedNode.Item.FullName;
    }

    private void CancelButton_OnClick(object? sender, RoutedEventArgs e)
    {
        ViewModel.Dispose();
        Close();
    }

    private void Layoutable_OnEffectiveViewportChanged(object? sender, EffectiveViewportChangedEventArgs e)
    {
        AligningSeparator.Width = TopMostGrid.Bounds.Width - BottomButtonsStackPanel.Bounds.Width;
    }

    private async void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button { DataContext: AdvancedTaggingDialogViewModel viewModel }) return;

        var options = new FolderPickerOpenOptions { Title = "Select folder", AllowMultiple = true };

        var result = await GetStorageProvider().OpenFolderPickerAsync(options);

        if (result.Count == 0) return;

        foreach (var folder in result)
        {
            viewModel.AddItemCommand.Execute(new DirectoryInfo(folder.Path.LocalPath));
        }
    }

    private IStorageProvider GetStorageProvider() => ((TopLevel)VisualRoot!).StorageProvider;
}
