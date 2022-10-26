using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using TagTool.App.Extensions;
using TagTool.App.ViewModels.Dialogs;
using TagTool.App.ViewModels.UserControls;

namespace TagTool.App.Views.Dialogs;

public partial class AddFileDialog : Window
{
    private readonly AddFileDialogViewModel _vm = Application.Current?.CreateInstance<AddFileDialogViewModel>()!;

    public AddFileDialog()
    {
        DataContext = _vm;
        InitializeComponent();

        // IStorageFolder? lastSelectedDirectory = null;
    }

    private async void OpenFilePickerButton_OnClick(object? sender, RoutedEventArgs e)
    {
        var options = new FilePickerOpenOptions { Title = "Open file", FileTypeFilter = new[] { FilePickerFileTypes.All }, AllowMultiple = false };

        var result = await GetStorageProvider().OpenFilePickerAsync(options);

        if (result.Count == 0) return;

        SelectFileTextBox.Text = result[0].Name;
    }

    private IStorageProvider GetStorageProvider()
    {
        var visualRoot = VisualRoot as TopLevel ?? throw new ArgumentException("Invalid Owner");

        return visualRoot.StorageProvider;
    }

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
        var simpleTagsBarViewModel = TagsToApplySimpleTagsBar.DataContext as SimpleTagsBarViewModel
                                     ?? throw new InvalidCastException("Expected different DataContext");

        Close((SelectFileTextBox.Text, simpleTagsBarViewModel.Tags));
    }
}
