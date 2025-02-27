﻿using System.Collections.ObjectModel;
using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;
using DynamicData;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using TagTool.App.Contracts;
using TagTool.App.Extensions;
using TagTool.App.Models;
using TagTool.App.Services;
using TagTool.BackendNew;

namespace TagTool.App.Views;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly TaggableItemIconResolver _iconResolver;
    private readonly TaggableItemDisplayTextResolver _displayTextResolver;
    private readonly TaggableItemMapper _taggableItemMapper;
    private readonly TagService.TagServiceClient _tagService;

    public TaggableItemsSearchBarViewModel SearchBarViewModel { get; }

    public ObservableCollection<TaggableItemViewModel> SearchResults { get; } = [];

    public Dictionary<Type, string[]> TaggableItemContextMenuActions { get; set; } = [];

    /// <summary>
    ///     ctor for XAML previewer
    /// </summary>
    public MainWindowViewModel()
    {
        if (!Design.IsDesignMode)
        {
            Debug.Fail("ctor for XAML Previewer should not be invoke during standard execution");
        }

        _displayTextResolver = AppTemplate.Current.Services.GetRequiredService<TaggableItemDisplayTextResolver>();
        _iconResolver = AppTemplate.Current.Services.GetRequiredService<TaggableItemIconResolver>();
        _taggableItemMapper = AppTemplate.Current.Services.GetRequiredService<TaggableItemMapper>();
        _tagService = AppTemplate.Current.Services.GetRequiredService<ITagToolBackend>().GetTagService();

        SearchBarViewModel = AppTemplate.Current.Services.GetRequiredService<TaggableItemsSearchBarViewModel>();

        Initialize();
    }

    [UsedImplicitly]
    public MainWindowViewModel(
        TaggableItemsSearchBarViewModel taggableItemsSearchBarViewModel,
        ITagToolBackend tagToolBackend,
        TaggableItemDisplayTextResolver displayTextResolver,
        TaggableItemIconResolver iconResolver,
        TaggableItemMapper taggableItemMapper)
    {
        _tagService = tagToolBackend.GetTagService();
        _displayTextResolver = displayTextResolver;
        _iconResolver = iconResolver;
        _taggableItemMapper = taggableItemMapper;
        SearchBarViewModel = taggableItemsSearchBarViewModel;

        Initialize();
    }

    private void Initialize()
    {
        SearchBarViewModel.CommitSearchQueryEvent += (_, args) => Dispatcher.UIThread.InvokeAsync(() => SearchForTaggableItems(args.QuerySegments));

        // Initial, empty search.
        Dispatcher.UIThread.InvokeAsync(() => SearchForTaggableItems(null));
        // Fetch TaggableItem-specific operations
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            var reply = _tagService.GetOperations(new GetOperationsRequest());
            TaggableItemContextMenuActions = reply.Operations.ToDictionary(
                o => o.TypeName == "TaggableFile_A8ABBA71" ? typeof(TaggableFile.TaggableFile) : throw new ArgumentOutOfRangeException(),
                o => o.Name.ToArray());
        });
    }

    [RelayCommand]
    private void ShowNewItemsPanel()
    {
    }

    [RelayCommand]
    private async Task TagItem((TaggableItemBase Item, Tag Tag) args)
    {
        var (item, tag) = args;
        var reply = await _tagService.TagItemAsync(new TagItemRequest { TagId = tag.Id, ItemId = item.Id.ToString() });

        switch (reply.ResultCase)
        {
            case TagItemReply.ResultOneofCase.TaggableItem:
                item.Tags = reply.TaggableItem.Tags.MapFromDto().ToHashSet();
                break;
            case TagItemReply.ResultOneofCase.ErrorMessage:
                Debug.WriteLine("Unable to tag item");
                break;
            default:
                throw new UnreachableException();
        }
    }

    [RelayCommand]
    private async Task UntagItem((TaggableItemBase Item, Tag Tag) args)
    {
        var (item, tag) = args;
        var reply = await _tagService.UntagItemAsync(new UntagItemRequest { TagId = tag.Id, ItemId = item.Id.ToString() });

        // switch (reply.ResultCase)
        // {
        //     case UntagItemReply.ResultOneofCase.TaggedItem:
        //         item.Tags = reply.TaggedItem.Tags.MapFromDto().ToHashSet();
        //         break;
        //     case UntagItemReply.ResultOneofCase.ErrorMessage:
        //         Debug.WriteLine("Unable to untag item");
        //         break;
        //     case UntagItemReply.ResultOneofCase.None:
        //     default:
        //         throw new UnreachableException();
        // }
    }

    private async Task SearchForTaggableItems(ICollection<QuerySegment>? argsQuerySegments)
    {
        var tagQueryParams = argsQuerySegments?.Select(segment => segment.MapToDto());

        var reply = await _tagService.GetItemsByTagsAsync(new GetItemsByTagsRequest { QueryParams = { tagQueryParams ?? [] } });

        var taggableItems = reply.TaggedItems
            .Select(i => _taggableItemMapper.MapToObj(i.Item.Type, i.Item.Payload, i.Tags))
            .ToArray();

        SearchResults.Clear();
        SearchResults.AddRange(taggableItems.Select(
            item =>
            {
                var text = _displayTextResolver.GetDisplayText(item);
                var icon = _iconResolver.GetIcon(item, null);
                return new TaggableItemViewModel(item, icon, text);
            }));
    }
}
