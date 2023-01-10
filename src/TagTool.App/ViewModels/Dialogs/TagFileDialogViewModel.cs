using System.Collections.ObjectModel;
using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Platform.Storage;
using Avalonia.Platform.Storage.FileIO;
using CommunityToolkit.Mvvm.ComponentModel;
using JetBrains.Annotations;
using Microsoft.Extensions.Options;
using TagTool.App.Core.Models;
using TagTool.App.Options;

namespace TagTool.App.ViewModels.Dialogs;

public partial class TagFileDialogViewModel : ViewModelBase
{
    private readonly GeneralOptions _options;

    [ObservableProperty]
    private string _text = "";

    public ObservableCollection<Tag> ImplicitTags { get; set; } = new(new Tag[] { new("Audio"), new("Text"), new("Date"), new("Zip") });

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
        if (!Design.IsDesignMode)
        {
            Debug.Fail("ctor for XAML Previewer should not be invoke during standard execution");
        }

        _options = null!; // to suppress warning
    }

    [UsedImplicitly]
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
