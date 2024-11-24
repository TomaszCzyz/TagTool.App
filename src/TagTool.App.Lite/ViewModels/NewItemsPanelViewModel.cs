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
    private readonly FileActionsService.FileActionsServiceClient _fileActionsService;

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
        _fileActionsService = AppTemplate.Current.Services.GetRequiredService<ITagToolBackend>().GetFileActionsService();

        Initialize();
    }

    [UsedImplicitly]
    public NewItemsPanelViewModel(ILogger<NewItemsPanelViewModel> logger, ITagToolBackend tagToolBackend)
    {
        _logger = logger;
        _fileActionsService = tagToolBackend.GetFileActionsService();

        Initialize();
    }

    private void Initialize()
    {
        Dispatcher.UIThread.InvokeAsync(ScanWatchedLocation);
    }

    private async Task ScanWatchedLocation()
    {
        var reply = await _fileActionsService.DetectNewItemsAsync(new DetectNewItemsRequest());
        if (reply.Error is not null)
        {
            NoObservedLocation = true;
            return;
        }

        ItemsToTag.AddRange(reply.Items.Select(dto => dto.File.Path));
    }

    [RelayCommand]
    private async Task AddWatchedLocation(string path)
    {
        var reply = await _fileActionsService.AddWatchedLocationAsync(new AddWatchedLocationRequest { Path = path });

        switch (reply.ResultCase)
        {
            case AddWatchedLocationReply.ResultOneofCase.None:
                _logger.LogDebug("Successfully added a new watched location with path {Path}", path);
                break;
            case AddWatchedLocationReply.ResultOneofCase.Error:
                _logger.LogWarning("Failed to add a new watched location with path {Path}", path);
                AddLocationErrorMessage = reply.Error.Message;
                break;
            default:
                throw new UnreachableException();
        }

        await ScanWatchedLocation();
    }
}
