using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Microsoft.Extensions.DependencyInjection;
using TagTool.App.Core;
using TagTool.App.Lite.ViewModels;

namespace TagTool.App.Lite.Views;

public partial class NewItemsPanel : UserControl
{
    private readonly NewItemsPanelViewModel _viewModel = AppTemplate.Current.Services.GetRequiredService<NewItemsPanelViewModel>();

    public NewItemsPanel()
    {
        DataContext = _viewModel;

        InitializeComponent();
    }

    private async void SelectNewWatchedLocation_OnClick(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button { DataContext: NewItemsPanelViewModel viewModel })
        {
            return;
        }

        var storageProvider = GetStorageProvider();

        var downloadDir = await storageProvider.TryGetWellKnownFolderAsync(WellKnownFolder.Downloads);

        var options = new FolderPickerOpenOptions
        {
            Title = "Select 'Watched Location'",
            AllowMultiple = false,
            SuggestedStartLocation = downloadDir
        };

        var result = await storageProvider.OpenFolderPickerAsync(options);

        if (result.Count == 0)
        {
            return;
        }

        foreach (var folder in result)
        {
            viewModel.AddWatchedLocationCommand.Execute(folder.Path.LocalPath);
        }
    }

    private IStorageProvider GetStorageProvider() => ((TopLevel)VisualRoot!).StorageProvider;

    private void InputElement_OnGotFocus(object? sender, GotFocusEventArgs e)
    {
        Debug.WriteLine("GotFocus");
    }
}
