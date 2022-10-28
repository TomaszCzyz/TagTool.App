using Avalonia.Data;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Options;
using TagTool.App.Options;

namespace TagTool.App.ViewModels.Dialogs;

public partial class AddFileDialogViewModel : ObservableObject
{
    private readonly IOptions<GeneralOptions> _options;

    [ObservableProperty]
    private string _text = "";

    /// <summary>
    ///     ctor for XAML previewer
    /// </summary>
    public AddFileDialogViewModel()
    {
        _options = null!; // to suppress warning
    }

    public AddFileDialogViewModel(IOptions<GeneralOptions> options)
    {
        _options = options;
    }

    partial void OnTextChanging(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || !File.Exists(value))
        {
            throw new DataValidationException("File does not exists");
        }
    }
}
