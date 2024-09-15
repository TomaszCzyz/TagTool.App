using System.Collections.ObjectModel;
using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Dock.Model.Mvvm.Controls;
using DynamicData;
using Google.Protobuf.WellKnownTypes;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using TagTool.App.Core.Models;
using TagTool.App.Core.Services;
using TagTool.App.Core.ViewModels;
using TagTool.App.Models;
using TagTool.Backend;
using TagTool.Backend.DomainTypes;

namespace TagTool.App.ViewModels.UserControls;

public partial class TaggableItemsSearchViewModel : Document
{
    private readonly ILogger<TaggableItemsSearchViewModel> _logger;
    private readonly TagService.TagServiceClient _tagService;
    private readonly FileActionsService.FileActionsServiceClient _fileActionsService;

    [ObservableProperty]
    private Tag? _selectedItemFromSearched;

    public TaggableItemsSearchBarViewModel SearchBarViewModel { get; }

    public ObservableCollection<TaggableItemViewModel> FoundTaggedItems { get; set; } = [];

    /// <summary>
    ///     ctor for XAML previewer
    /// </summary>
    public TaggableItemsSearchViewModel()
    {
        if (!Design.IsDesignMode)
        {
            Debug.Fail("ctor for XAML Previewer should not be invoke during standard execution");
        }

        var grpcChannelFactory = new GrpcChannelFactory();
        var tagToolBackend = new TagToolBackend(grpcChannelFactory);

        _logger = null!;
        _tagService = tagToolBackend.GetTagService();
        _fileActionsService = tagToolBackend.GetFileActionsService();
        SearchBarViewModel = new TaggableItemsSearchBarViewModel();

        Initialize();
    }

    [UsedImplicitly]
    public TaggableItemsSearchViewModel(
        ILogger<TaggableItemsSearchViewModel> logger,
        ITagToolBackend tagToolBackend,
        TaggableItemsSearchBarViewModel taggableItemsSearchBarViewModel)
    {
        _logger = logger;
        _tagService = tagToolBackend.GetTagService();
        _fileActionsService = tagToolBackend.GetFileActionsService();
        SearchBarViewModel = taggableItemsSearchBarViewModel;

        Initialize();
    }

    private void Initialize()
    {
        SearchBarViewModel.CommitSearchQueryEvent += (_, args) => Dispatcher.UIThread.InvokeAsync(() => SearchForTaggableItems(args.QuerySegments));

        // Initial, empty search to retrieve the most popular items.
        Dispatcher.UIThread.InvokeAsync(() => SearchForTaggableItems(null));
    }

    private async Task SearchForTaggableItems(ICollection<QuerySegment>? querySegments)
    {
        var tagQueryParams = querySegments?.Select(segment => segment.MapToDto());

        var reply = await _tagService.GetItemsByTagsAsync(new GetItemsByTagsRequest
        {
            QueryParams = { tagQueryParams ?? Array.Empty<TagQueryParam>() }
        });

        FoundTaggedItems.Clear();
        FoundTaggedItems.AddRange(reply.TaggedItems.Select(item
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

    public async Task<List<FileSystemInfo>> VerifyItemsToAdd(IEnumerable<FileSystemInfo> infos)
    {
        var alreadyTaggedItems = new List<FileSystemInfo>();
        foreach (var info in infos)
        {
            var taggableItemDto = info switch
            {
                DirectoryInfo dirInfo => new TaggableItemDto { File = new FileDto { Path = dirInfo.FullName } },
                FileInfo fileInfo => new TaggableItemDto { Folder = new FolderDto { Path = fileInfo.FullName } },
                _ => throw new ArgumentOutOfRangeException(nameof(infos))
            };

            var reply = await _tagService.GetItemAsync(new GetItemRequest { TaggableItemDto = taggableItemDto });

            if (reply.ResultCase is GetItemReply.ResultOneofCase.TaggedItem && reply.TaggedItem.Tags.Count != 0)
            {
                alreadyTaggedItems.Add(info);
            }
        }

        return alreadyTaggedItems;
    }

    [RelayCommand]
    private async Task AddNewItems((IEnumerable<FileSystemInfo> Infos, bool MoveToInternalStorage) input)
    {
        foreach (var info in input.Infos)
        {
            const string tagName = "JustAdded";

            var any = Any.Pack(new NormalTag { Name = tagName });
            var tagRequest = info switch
            {
                DirectoryInfo dirInfo => new TaggableItemDto { Folder = new FolderDto { Path = dirInfo.FullName } },
                FileInfo fileInfo => new TaggableItemDto { File = new FileDto { Path = fileInfo.FullName } },
                _ => throw new UnreachableException()
            };

            var reply = await _tagService.TagItemAsync(new TagItemRequest { Tag = any, Item = tagRequest });

            if (input.MoveToInternalStorage && tagRequest.ItemCase is TaggableItemDto.ItemOneofCase.File)
            {
                await MoveToInternalStorage(info, tagRequest.File);
            }

            switch (reply.ResultCase)
            {
                case TagItemReply.ResultOneofCase.Item:
                    break;
                case TagItemReply.ResultOneofCase.ErrorMessage:
                    _logger.LogWarning("Unable to add tag item {Item} with a tag {TagName} to", tagRequest.ItemCase, tagName);
                    continue;
                default:
                    throw new ArgumentOutOfRangeException(nameof(input));
            }
        }

        // await CommitSearch();
    }

    private async Task MoveToInternalStorage(FileSystemInfo info, FileDto item)
    {
        switch (info)
        {
            case FileInfo:
                var request = new MoveFileRequest { File = item, Destination = "CommonStorage" };
                var _ = await _fileActionsService.MoveFileAsync(request);
                break;
        }
    }
}
