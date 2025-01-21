using System.Collections.Generic;
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
using TagTool.App.Core;
using TagTool.App.Core.Models;
using TagTool.App.Core.Services;
using TagTool.App.Core.Views;
using TagTool.BackendNew;

namespace TagTool.App.Lite.Views;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly TaggableItemIconResolverDispatcher _iconResolver;
    private readonly TaggableItemDisplayTextResolver _displayTextResolver;
    private readonly TaggableItemMapper _taggableItemMapper;
    private readonly TagService.TagServiceClient _tagService;

    [ObservableProperty]
    private bool _isNewItemsPanelVisible;

    public TaggableItemsSearchBarViewModel SearchBarViewModel { get; }

    public ObservableCollection<TaggableItemModel> SearchResults { get; } = [];

    public ObservableCollection<TaggableItemModel> OtherResults { get; set; } = [];

    /// <summary>
    ///     ctor for XAML previewer
    /// </summary>
    public MainWindowViewModel(TaggableItemDisplayTextResolver displayTextResolver, TaggableItemIconResolverDispatcher iconResolver,
        TaggableItemMapper taggableItemMapper)
    {
        if (!Design.IsDesignMode)
        {
            Debug.Fail("ctor for XAML Previewer should not be invoke during standard execution");
        }

        _displayTextResolver = displayTextResolver;
        _iconResolver = iconResolver;
        _taggableItemMapper = taggableItemMapper;

        _tagService = AppTemplate.Current.Services.GetRequiredService<ITagToolBackend>().GetTagService();
        SearchBarViewModel = AppTemplate.Current.Services.GetRequiredService<TaggableItemsSearchBarViewModel>();

        Initialize();
    }

    [UsedImplicitly]
    public MainWindowViewModel(TaggableItemsSearchBarViewModel taggableItemsSearchBarViewModel, ITagToolBackend tagToolBackend,
        TaggableItemDisplayTextResolver displayTextResolver, TaggableItemIconResolverDispatcher iconResolver, TaggableItemMapper taggableItemMapper)
    {
        _tagService = tagToolBackend.GetTagService();
        SearchBarViewModel = taggableItemsSearchBarViewModel;
        _displayTextResolver = displayTextResolver;
        _iconResolver = iconResolver;
        _taggableItemMapper = taggableItemMapper;

        Initialize();
    }

    private void Initialize()
    {
        SearchBarViewModel.CommitSearchQueryEvent += (_, args) => Dispatcher.UIThread.InvokeAsync(() => SearchForTaggableItems(args.QuerySegments));

        // Initial, empty search to retrieve the most popular items.
        Dispatcher.UIThread.InvokeAsync(() => SearchForTaggableItems(null));

        // OtherResults.Add(new TaggableItemViewModel(_tagService, )
        // {
        //     TaggableItem = new TaggableFile { Path = "info.FullName" }, AreTagsVisible = true
        // });
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

    [RelayCommand]
    private void ShowNewItemsPanel()
    {
        IsNewItemsPanelVisible ^= true;
    }

    [RelayCommand]
    private async Task TagItem((TaggableItem Item, Tag Tag) args)
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
    private async Task UntagItem((TaggableItem Item, Tag Tag) args)
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
            .Select(i => _taggableItemMapper.MapToObj(i.Item.Type, i.Item.Payload))
            .ToArray();

        SearchResults.Clear();
        SearchResults.AddRange(taggableItems.Select(
            item =>
            {
                var text = _displayTextResolver.GetDisplayText(item);
                var icon = _iconResolver.GetIcon(item, null);
                var tags = item.Tags?.ToHashSet() ?? [];
                return new TaggableItemModel(item.Id, text, icon, tags);
            }));
    }
}
