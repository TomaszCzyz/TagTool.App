using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using Avalonia.Platform.Storage.FileIO;
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

        if (result.Count == 0 || !result[0].TryGetUri(out var filePath)) return;

        var folderPath = Directory.GetParent(filePath.LocalPath)?.FullName;
        ViewModel.FilePickerSuggestedStartLocation = folderPath is null ? null : new BclStorageFolder(folderPath);
        SelectFileTextBox.Text = filePath.LocalPath;
    }

    private IStorageProvider GetStorageProvider() => ((TopLevel)VisualRoot!).StorageProvider;

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

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
