using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using TagTool.App.ViewModels.Dialogs;
using TagTool.App.ViewModels.UserControls;

namespace TagTool.App.Views.Dialogs;

public partial class TagFileDialog : Window
{
    private TagFileDialogViewModel ViewModel => (TagFileDialogViewModel)DataContext!;

    public TagFileDialog(string startLocation) : this()
    {
        ViewModel.Text = startLocation;
    }

    public TagFileDialog()
    {
        InitializeComponent();
    }

    private async void OpenFilePickerButton_OnClick(object? sender, RoutedEventArgs e)
    {
        var options = new FilePickerOpenOptions
        {
            Title = "Select file",
            FileTypeFilter = new[] { FilePickerFileTypes.All },
            AllowMultiple = false,
            SuggestedStartLocation = ViewModel.FilePickerSuggestedStartLocation
        };

        var result = await GetStorageProvider().OpenFilePickerAsync(options);

        if (result.Count == 0) return;

        var filePath = result[0].Path;
        var _ = Directory.GetParent(filePath.LocalPath)?.FullName;
        // ViewModel.FilePickerSuggestedStartLocation = folderPath is null ? null : new BclStorageFolder(folderPath);
        SelectFileTextBox.Text = filePath.LocalPath;
    }

    private IStorageProvider GetStorageProvider() => ((TopLevel)VisualRoot!).StorageProvider;

    private void CancelButton_OnClick(object? sender, RoutedEventArgs e)
    {
        Close();
    }

    private void TagButton_OnClick(object? sender, RoutedEventArgs e)
    {
        var simpleTagsBarViewModel = (SimpleTagsBarViewModel)TagsToApplySimpleTagsBar.DataContext!;

        Close((SelectFileTextBox.Text, simpleTagsBarViewModel.Tags));
    }
}
