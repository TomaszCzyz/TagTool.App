using System.Collections.ObjectModel;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DynamicData;
using Grpc.Core;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using TagTool.App.Core.Models;
using TagTool.App.Core.Services;
using TagTool.Backend;

namespace TagTool.App.ViewModels.UserControls;

public partial class TaggedItemsSearchViewModel : ViewModelBase, IDisposable
{
    private readonly TagSearchService.TagSearchServiceClient _tagSearchService;
    private readonly TagService.TagServiceClient _tagService;
    private readonly IWordHighlighter _wordHighlighter;

    [ObservableProperty]
    private string? _searchText;

    [ObservableProperty]
    private Tag? _selectedItemFromSearched;

    [ObservableProperty]
    private Tag? _selectedItemFromPopular;

    public Tag? SelectedItemFromPopup => SelectedItemFromSearched ?? SelectedItemFromPopular;

    public ObservableCollection<Tag> SearchResults { get; set; } = new();

    public ObservableCollection<Tag> PopularTags { get; set; } = new();

    public ObservableCollection<object> EnteredTags { get; set; } = new();

    public ObservableCollection<TaggableItemViewModel> Files { get; set; } = new();

    private IEnumerable<Tag> Tags => EnteredTags.Where(o => o.GetType() == typeof(Tag)).Cast<Tag>().ToArray();

    /// <summary>
    ///     ctor for XAML previewer
    /// </summary>
    public TaggedItemsSearchViewModel()
    {
        _tagSearchService = App.Current.Services.GetRequiredService<ITagToolBackend>().GetSearchService();
        _tagService = App.Current.Services.GetRequiredService<ITagToolBackend>().GetTagService();
        _wordHighlighter = App.Current.Services.GetRequiredService<IWordHighlighter>();
    }

    [UsedImplicitly]
    public TaggedItemsSearchViewModel(ITagToolBackend tagToolBackend, IWordHighlighter wordHighlighter)
    {
        _tagSearchService = tagToolBackend.GetSearchService();
        _tagService = tagToolBackend.GetTagService();
        _wordHighlighter = wordHighlighter;

        EnteredTags.CollectionChanged += async (_, _) => await Dispatcher.UIThread.InvokeAsync(CommitSearch);

        var tags = new Tag[] { new("Hello") };
        EnteredTags.AddRange(tags);

        var popularTags = new Tag[] { new("SomeTag"), new("Tag"), new("SomeTag"), new("Picture"), new("Tag"), new("Picture"), new("Picture") };
        PopularTags.AddRange(popularTags);

        var searchResults = new Tag[] { new("Result1"), new("Tag"), new("Result2"), new("Picture"), new("SearchTag") };
        SearchResults.AddRange(searchResults);

        EnteredTags.Add("");
    }

    [RelayCommand]
    private async Task CommitSearch()
    {
        var getItemsRequest = new GetItemsRequest { TagNames = { Tags.Select(tag => tag.Name).ToArray() } };
        var streamingCall = _tagService.GetItems(getItemsRequest);

        var results = new List<TaggableItemViewModel>();
        while (await streamingCall.ResponseStream.MoveNext())
        {
            var reply = streamingCall.ResponseStream.Current;
            var tags = reply.TagNames.Select(s => new Tag(s)).ToArray();

            TaggedItemType type;
            long? size;
            FileSystemInfo info;
            if (reply.FileInfo is not null)
            {
                type = TaggedItemType.File;
                info = new FileInfo(reply.FileInfo.Path);
                size = ((FileInfo)info).Length;
            }
            else
            {
                type = TaggedItemType.Folder;
                info = new DirectoryInfo(reply.FolderInfo.Path);
                size = null;
            }

            results.Add(
                new TaggableItemViewModel(_tagService)
                {
                    TaggedItemType = type,
                    DisplayName = info.Name,
                    Location = info.FullName,
                    DateCreated = info.CreationTime,
                    AreTagsVisible = true,
                    Size = size,
                    AssociatedTags = { tags }
                });
        }

        Files.Clear();
        Files.AddRange(results);
    }

    [RelayCommand]
    private void RemoveTag(Tag tag)
    {
        EnteredTags.Remove(tag);
    }

    [RelayCommand]
    private void RemoveLast()
    {
        var lastTag = EnteredTags.LastOrDefault(o => o.GetType() == typeof(Tag));

        if (lastTag is null) return;

        EnteredTags.Remove(lastTag);
    }

    [RelayCommand]
    private void UpdateSearch()
    {
        SelectedItemFromSearched = SearchResults.FirstOrDefault();
    }

    [RelayCommand]
    private void AddTag()
    {
        var itemToAdd = SelectedItemFromPopup;

        if (itemToAdd is null || EnteredTags.Contains(itemToAdd)) return;

        SearchText = "";
        SearchResults.Remove(itemToAdd);

        EnteredTags.Insert(EnteredTags.Count - 1, itemToAdd);
    }

    private CancellationTokenSource? _cts;

    partial void OnSearchTextChanged(string? value)
    {
        _cts?.Cancel(false);
        _cts?.Dispose();
        _cts = new CancellationTokenSource();

        async void Action() => await DoSearch(value, _cts.Token);

        Dispatcher.UIThread.InvokeAsync(Action, DispatcherPriority.MaxValue);
    }

    private async Task DoSearch(string? value, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(value)) return;

        SearchResults.Clear(); // todo: it breaks without throttle

        var matchTagsRequest = new MatchTagsRequest { PartialTagName = value, MaxReturn = 50 };
        var callOptions = new CallOptions().WithCancellationToken(ct);

        using var streamingCall = _tagSearchService.MatchTags(matchTagsRequest, callOptions);

        try
        {
            while (await streamingCall.ResponseStream.MoveNext(ct))
            {
                var reply = streamingCall.ResponseStream.Current;

                var highlightInfos = reply.MatchedParts
                    .Select(match => new HighlightInfo(match.StartIndex, match.Length))
                    .ToArray();

                var inlines = _wordHighlighter.CreateInlines(reply.MatchedTagName, highlightInfos);

                SearchResults.Add(new Tag(reply.MatchedTagName, inlines));
            }
        }
        catch (RpcException e) when (e.Status.StatusCode == StatusCode.Cancelled)
        {
            // this.Log().Debug("Streaming of tag names hints for SearchBar was cancelled");
        }
    }

    public void Dispose()
    {
        _cts?.Dispose();
        GC.SuppressFinalize(this);
    }
}
