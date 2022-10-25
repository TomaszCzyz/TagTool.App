using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using TagTool.App.Extensions;
using TagTool.App.ViewModels.Dialogs;

namespace TagTool.App.Views.Dialogs;

public partial class AddFileDialog : Window
{
    private readonly AddFileDialogViewModel _vm = Application.Current?.CreateInstance<AddFileDialogViewModel>()!;

    public AddFileDialog()
    {
        DataContext = _vm;
        InitializeComponent();

        IStorageFolder? lastSelectedDirectory = null;

        var selectedFileTextBox = this.Get<TextBox>(nameof(SelectFileTextBox));

        this.Get<Button>(nameof(OpenFilePickerButton)).Click += async delegate
        {
            var result = await GetStorageProvider().OpenFilePickerAsync(
                new FilePickerOpenOptions
                {
                    Title = "Open file",
                    FileTypeFilter = new[] { FilePickerFileTypes.All },
                    SuggestedStartLocation = lastSelectedDirectory,
                    AllowMultiple = false
                });

            if (result.Count == 0) return;

            selectedFileTextBox.Text = result[0].Name;
        };
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
}
