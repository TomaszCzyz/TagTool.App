using Avalonia.Data;
using CommunityToolkit.Mvvm.ComponentModel;

namespace TagTool.App.ViewModels.Dialogs;

public partial class AddFileDialogViewModel : ObservableObject
{
    [ObservableProperty]
    private string _text = "";

    partial void OnTextChanging(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || !File.Exists(value))
        {
            throw new DataValidationException("File does not exists");
        }
    }
}
