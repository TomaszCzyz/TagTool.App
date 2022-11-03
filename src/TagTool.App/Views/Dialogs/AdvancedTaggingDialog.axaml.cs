using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using Microsoft.Extensions.DependencyInjection;
using TagTool.App.ViewModels.Dialogs;
using TagTool.App.ViewModels.UserControls;

namespace TagTool.App.Views.Dialogs;

public partial class AdvancedTaggingDialog : Window
{
    private readonly AdvancedTaggingDialogViewModel _viewModel = App.Current.Services.GetRequiredService<AdvancedTaggingDialogViewModel>();

    public AdvancedTaggingDialog()
    {
        DataContext = _viewModel;
        InitializeComponent();
#if DEBUG
        this.AttachDevTools();
#endif
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void TreeView_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if(!PreviewToggleButton.IsChecked ?? false) return;
        if (TreeView.SelectedItems is not { Count: 1 } selectedItems) return;

        var selectedNode = selectedItems[0] as AdvancedTaggingDialogViewModel.Node
                           ?? throw new InvalidCastException(
                               $"Expected item to be {nameof(AdvancedTaggingDialogViewModel.Node)}, got {selectedItems[0]!.GetType()}");

        var previewerViewModel = FilePreviewer?.DataContext as FilePreviewerViewModel
                                 ?? throw new InvalidCastException(
                                     $"Expected FilePreviewer DataContext to be {nameof(FilePreviewerViewModel)}," +
                                     $" got {FilePreviewer?.DataContext?.GetType()}");

        previewerViewModel.CurrentFilePath = selectedNode.Item.FullName;
    }

    private void CancelButton_OnClick(object? sender, RoutedEventArgs e)
    {
        _viewModel.Dispose();
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
            if (!folder.TryGetUri(out var folderPath)) continue;

            viewModel.AddItemCommand.Execute(new DirectoryInfo(folderPath.LocalPath));
        }
    }

    private IStorageProvider GetStorageProvider()
    {
        var visualRoot = VisualRoot as TopLevel ?? throw new ArgumentException("Invalid Owner");

        return visualRoot.StorageProvider;
    }
}
