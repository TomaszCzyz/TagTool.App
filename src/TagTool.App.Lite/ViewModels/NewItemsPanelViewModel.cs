using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DynamicData;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TagTool.App.Core;
using TagTool.App.Core.Services;
using TagTool.App.Core.ViewModels;
using TagTool.Backend;

namespace TagTool.App.Lite.ViewModels;

public partial class NewItemsPanelViewModel : ViewModelBase
{
    private readonly ILogger<NewItemsPanelViewModel> _logger;

    public ObservableCollection<string> ItemsToTag { get; } = [];

    [ObservableProperty]
    private bool _noObservedLocation;

    [ObservableProperty]
    private string? _addLocationErrorMessage;

    /// <summary>
    ///     ctor for XAML previewer
    /// </summary>
    public NewItemsPanelViewModel()
    {
        if (!Design.IsDesignMode)
        {
            Debug.Fail("ctor for XAML Previewer should not be invoke during standard execution");
        }

        _logger = AppTemplate.Current.Services.GetRequiredService<ILogger<NewItemsPanelViewModel>>();

        Initialize();
    }

    [UsedImplicitly]
    public NewItemsPanelViewModel(ILogger<NewItemsPanelViewModel> logger, ITagToolBackend tagToolBackend)
    {
        _logger = logger;

        Initialize();
    }

    private void Initialize()
    {
        Dispatcher.UIThread.InvokeAsync(ScanWatchedLocation);
        Dispatcher.UIThread.InvokeAsync(GetWatchedLocations);
    }

    private async Task ScanWatchedLocation()
    {
        // if (reply.Error is not null)
        // {
        //     _logger.LogWarning("Failed to detect new items, {@Error}", reply.Error);
        //     return;
        // }

        // ItemsToTag.AddRange(reply.Items.Select(dto => dto.File.Path));
    }

    private async Task GetWatchedLocations()
    {
        // var reply = await _fileActionsService.GetWatchedLocationsAsync(new GetWatchedLocationsRequest());
        //
        // if (reply.Paths.Count == 0)
        // {
        //     NoObservedLocation = true;
        // }
    }

    [RelayCommand]
    private async Task AddWatchedLocation(string path)
    {
        // var reply = await _fileActionsService.AddWatchedLocationAsync(new AddWatchedLocationRequest { Path = path });
        //
        // switch (reply.ResultCase)
        // {
        //     case AddWatchedLocationReply.ResultOneofCase.None:
        //         _logger.LogDebug("Successfully added a new watched location with path {Path}", path);
        //         NoObservedLocation = false;
        //         break;
        //     case AddWatchedLocationReply.ResultOneofCase.Error:
        //         _logger.LogWarning("Failed to add a new watched location with path {Path}", path);
        //         AddLocationErrorMessage = reply.Error.Message;
        //         break;
        //     default:
        //         throw new UnreachableException();
        // }

        await ScanWatchedLocation();
    }
}
