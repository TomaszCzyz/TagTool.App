using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using DynamicData;
using Google.Protobuf.WellKnownTypes;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
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

    [ObservableProperty]
    private TaggableItemsSearchBarViewModel _searchBarViewModel;

    public ObservableCollection<TaggableItemViewModel> SearchResults { get; } = new();

    /// <summary>
    ///     ctor for XAML previewer
    /// </summary>
    public MainWindowViewModel()
    {
        if (!Design.IsDesignMode)
        {
            Debug.Fail("ctor for XAML Previewer should not be invoke during standard execution");
        }

        _tagService = App.Current.Services.GetRequiredService<ITagToolBackend>().GetTagService();
        SearchBarViewModel = App.Current.Services.GetRequiredService<TaggableItemsSearchBarViewModel>();

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
    }

    private async Task SearchForTaggableItems(ICollection<QuerySegment> argsQuerySegments)
    {
        var tagQueryParams = argsQuerySegments.Select(segment
            => new GetItemsByTagsV2Request.Types.TagQueryParam { Tag = Any.Pack(TagMapper.MapToDto(segment.Tag)), State = MapQuerySegmentState(segment) });

        var reply = await _tagService.GetItemsByTagsV2Async(new GetItemsByTagsV2Request { QueryParams = { tagQueryParams } });

        SearchResults.Clear();
        SearchResults.AddRange(reply.TaggedItems.Select(item
            => new TaggableItemViewModel(_tagService)
            {
                TaggableItem = item.ItemCase switch
                {
                    TaggedItem.ItemOneofCase.File => new TaggableFile { Path = item.File.Path },
                    TaggedItem.ItemOneofCase.Folder => new TaggableFolder { Path = item.Folder.Path },
                    _ => throw new UnreachableException()
                },
                AreTagsVisible = true
            }));
    }

    private static GetItemsByTagsV2Request.Types.QuerySegmentState MapQuerySegmentState(QuerySegment segment) => segment.State switch
    {
        QuerySegmentState.Exclude => GetItemsByTagsV2Request.Types.QuerySegmentState.Exclude,
        QuerySegmentState.Include => GetItemsByTagsV2Request.Types.QuerySegmentState.Include,
        QuerySegmentState.MustBePresent => GetItemsByTagsV2Request.Types.QuerySegmentState.MustBePresent,
        _ => throw new UnreachableException()
    };
}
