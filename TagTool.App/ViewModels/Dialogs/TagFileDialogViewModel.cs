using Avalonia.Data;
using Avalonia.Platform.Storage;
using Avalonia.Platform.Storage.FileIO;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Options;
using TagTool.App.Options;

namespace TagTool.App.ViewModels.Dialogs;

public partial class TagFileDialogViewModel : ObservableObject
{
    private readonly GeneralOptions _options;

    [ObservableProperty]
    private string _text = "";

    public IStorageFolder? FilePickerSuggestedStartLocation
    {
        get
        {
            if (_options.FilePickerStartFolder is null) return new BclStorageFolder(nameof(Environment.SpecialFolder.Desktop));

            return _options.FilePickerStartFolderMode switch
            {
                FilePickerStartFolderMode.Fixed => new BclStorageFolder(_options.FilePickerStartFolder),
                FilePickerStartFolderMode.Previous => new BclStorageFolder(_options.FilePickerStartFolder),
                _ => new BclStorageFolder(nameof(Environment.SpecialFolder.Desktop))
            };
        }
        set
        {
            //todo: set "previous start location" user setting
        }
    }

    /// <summary>
    ///     ctor for XAML previewer
    /// </summary>
    public TagFileDialogViewModel()
    {
        _options = null!; // to suppress warning
    }

    public TagFileDialogViewModel(IOptions<GeneralOptions> options)
    {
        _options = options.Value;
    }

    partial void OnTextChanging(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || !File.Exists(value))
        {
            throw new DataValidationException("File does not exists");
        }
    }
}
