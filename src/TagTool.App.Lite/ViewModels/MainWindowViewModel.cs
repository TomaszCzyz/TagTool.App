using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;
using DynamicData;
using Google.Protobuf.WellKnownTypes;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using TagTool.App.Core;
using TagTool.App.Core.Models;
using TagTool.App.Core.Services;
using TagTool.App.Core.TagMapper;
using TagTool.App.Core.ViewModels;
using TagTool.Backend;
using TagTool.Backend.DomainTypes;

namespace TagTool.App.Lite.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly TagService.TagServiceClient _tagService;

    public TaggableItemsSearchBarViewModel SearchBarViewModel { get; }

    public ObservableCollection<TaggableItemViewModel> SearchResults { get; } = new();

    public ObservableCollection<TaggableItemViewModel> OtherResults { get; set; } = new();

    /// <summary>
    ///     ctor for XAML previewer
    /// </summary>
    public MainWindowViewModel()
    {
        if (!Design.IsDesignMode)
        {
            Debug.Fail("ctor for XAML Previewer should not be invoke during standard execution");
        }

        _tagService = AppTemplate.Current.Services.GetRequiredService<ITagToolBackend>().GetTagService();
        SearchBarViewModel = AppTemplate.Current.Services.GetRequiredService<TaggableItemsSearchBarViewModel>();

        Initialize();
    }

    [UsedImplicitly]
    public MainWindowViewModel(TaggableItemsSearchBarViewModel taggableItemsSearchBarViewModel, ITagToolBackend tagToolBackend)
    {
        _tagService = tagToolBackend.GetTagService();
        SearchBarViewModel = taggableItemsSearchBarViewModel;

        Initialize();
    }

    private void Initialize()
    {
        SearchBarViewModel.CommitSearchQueryEvent += (_, args) => Dispatcher.UIThread.InvokeAsync(() => SearchForTaggableItems(args.QuerySegments));

        // Initial, empty search to retrieve the most popular items.
        Dispatcher.UIThread.InvokeAsync(() => SearchForTaggableItems(null));

        OtherResults.Add(new TaggableItemViewModel(_tagService)
        {
            TaggableItem = new TaggableFile { Path = "info.FullName" }, AreTagsVisible = true
        });
    }

    [RelayCommand]
    private static async Task OpenTagToolApp()
    {
        await Task.Run(() =>
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    WorkingDirectory = @"C:\Users\tczyz\Source\Repos\My\TagTool\TagTool.App\src\TagTool.App\bin\Debug\net7.0",
                    FileName = @"C:\Users\tczyz\Source\Repos\My\TagTool\TagTool.App\src\TagTool.App\bin\Debug\net7.0\TagTool.App.exe",
                    UseShellExecute = true
                }
            };

            process.Start();
        });
    }

    private async Task SearchForTaggableItems(ICollection<QuerySegment>? argsQuerySegments)
    {
        var tagQueryParams = argsQuerySegments?.Select(segment => segment.MapToDto());

        var reply = await _tagService.GetItemsByTagsAsync(new GetItemsByTagsRequest
        {
            QueryParams = { tagQueryParams ?? Array.Empty<TagQueryParam>() }
        });

        SearchResults.Clear();
        SearchResults.AddRange(reply.TaggedItems.Select(item
            => new TaggableItemViewModel(_tagService)
            {
                TaggableItem = item.TaggableItem.ItemCase switch
                {
                    TaggableItemDto.ItemOneofCase.File => new TaggableFile { Path = item.TaggableItem.File.Path },
                    TaggableItemDto.ItemOneofCase.Folder => new TaggableFolder { Path = item.TaggableItem.Folder.Path },
                    _ => throw new UnreachableException()
                },
                AreTagsVisible = true
            }));
    }
}
