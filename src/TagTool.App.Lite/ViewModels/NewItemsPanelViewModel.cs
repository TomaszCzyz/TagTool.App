using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using DynamicData;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using TagTool.App.Core;
using TagTool.App.Core.Services;
using TagTool.App.Core.ViewModels;
using TagTool.Backend;

namespace TagTool.App.Lite.ViewModels;

public partial class NewItemsPanelViewModel : ViewModelBase
{
    private readonly FileActionsService.FileActionsServiceClient _fileActionsService;

    public ObservableCollection<string> ItemsToTag { get; } = new();

    [ObservableProperty]
    private bool _noObservedLocation;

    /// <summary>
    ///     ctor for XAML previewer
    /// </summary>
    public NewItemsPanelViewModel()
    {
        if (!Design.IsDesignMode)
        {
            Debug.Fail("ctor for XAML Previewer should not be invoke during standard execution");
        }

        _fileActionsService = AppTemplate.Current.Services.GetRequiredService<ITagToolBackend>().GetFileActionsService();

        Initialize();
    }

    [UsedImplicitly]
    public NewItemsPanelViewModel(ITagToolBackend tagToolBackend)
    {
        _fileActionsService = tagToolBackend.GetFileActionsService();

        Initialize();
    }

    private void Initialize()
    {
        Dispatcher.UIThread.InvokeAsync(async () =>
        {
            var reply = await _fileActionsService.DetectNewItemsAsync(new DetectNewItemsRequest());
            if (reply.Error is not null)
            {
                NoObservedLocation = true;
                return;
            }

            ItemsToTag.AddRange(reply.Items.Select(dto => dto.File.Path));
        });
    }
}
