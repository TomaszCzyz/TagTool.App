using System.Collections.ObjectModel;
using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Dock.Model.Mvvm.Controls;
using DynamicData;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TagTool.App.Core.Extensions;
using TagTool.App.Core.Models;
using TagTool.App.Core.Services;
using TagTool.App.Core.TagMapper;
using TagTool.App.Core.ViewModels;
using TagTool.App.Models;
using TagTool.Backend;
using TagTool.Backend.DomainTypes;

namespace TagTool.App.ViewModels.UserControls;

public class LogicalOperator
{
    public char Op { get; set; } = '|';
}

public partial class TaggableItemsSearchViewModel : Document, IDisposable
{
    private readonly ILogger<TaggableItemsSearchViewModel> _logger;
    private readonly TagService.TagServiceClient _tagService;
    private readonly FileActionsService.FileActionsServiceClient _fileActionsService;
    private readonly IWordHighlighter _wordHighlighter;
    private readonly ISpeechToTagSearchService _speechToTagSearchService;

    [ObservableProperty]
    private string _searchText = "";

    [ObservableProperty]
    private bool _areSpecialTagsVisible;

    [ObservableProperty]
    private Tag? _selectedItemFromSearched;

    [ObservableProperty]
    private Tag? _selectedItemFromPopular;

    public Tag? SelectedItemFromPopup => SelectedItemFromSearched ?? SelectedItemFromPopular;

    public ObservableCollection<Tag> SearchResults { get; set; } = new();

    public ObservableCollection<Tag> PopularTags { get; set; } = new();

    public ObservableCollection<object> EnteredTags { get; set; } = new();

    // todo: rename it Items or something like that
    public ObservableCollection<TaggableItemViewModel> Files { get; set; } = new();

    public ObservableCollection<string> AvailableSpacialTags { get; set; }
        = new(new[] { ".name", ".regex", ".olderThan", ".youngerThan", ".smallerThan" });

    private IEnumerable<Tag> Tags => EnteredTags.Where(o => o.GetType() == typeof(Tag)).Cast<Tag>().ToArray();

    /// <summary>
    ///     ctor for XAML previewer
    /// </summary>
    public TaggableItemsSearchViewModel()
    {
        if (!Design.IsDesignMode)
        {
            Debug.Fail("ctor for XAML Previewer should not be invoke during standard execution");
        }

        _logger = App.Current.Services.GetRequiredService<ILogger<TaggableItemsSearchViewModel>>();
        _tagService = App.Current.Services.GetRequiredService<ITagToolBackend>().GetTagService();
        _fileActionsService = App.Current.Services.GetRequiredService<ITagToolBackend>().GetFileActionsService();
        _wordHighlighter = App.Current.Services.GetRequiredService<IWordHighlighter>();
        _speechToTagSearchService = null!;

        Initialize();
    }

    [UsedImplicitly]
    public TaggableItemsSearchViewModel(
        ILogger<TaggableItemsSearchViewModel> logger,
        ITagToolBackend tagToolBackend,
        IWordHighlighter wordHighlighter,
        ISpeechToTagSearchService speechToTagSearchService)
    {
        _logger = logger;
        _tagService = tagToolBackend.GetTagService();
        _fileActionsService = tagToolBackend.GetFileActionsService();
        _wordHighlighter = wordHighlighter;
        _speechToTagSearchService = speechToTagSearchService;

        Initialize();
    }

    private void Initialize()
    {
        EnteredTags.CollectionChanged += async (_, _) => await Dispatcher.UIThread.InvokeAsync(CommitSearch);

        EnteredTags.Add(new Tag("Hello"));
        EnteredTags.Add(new LogicalOperator { Op = '|' });
        // EnteredTags.Add(new NameSpecialTag { FileName = "Hello" });

        var popularTags = new Tag[] { new("SomeTag"), new("Tag"), new("AnotherTag"), new("Picture"), new("PrettyTag"), new("Cold") };
        PopularTags.AddRange(popularTags);

        var searchResults = new Tag[] { new("Recipe"), new("Tag"), new("Chicken"), new("Picture"), new("SearchTag") };
        SearchResults.AddRange(searchResults);

        EnteredTags.Add("");
    }

    [RelayCommand]
    private async Task CommitSearch()
    {
        var query = Tags
            .Select(tag => Any.Pack(new NormalTag { Name = tag.Name }))
            .Select(any
                => new GetItemsByTagsV2Request.Types.TagQueryParam { Tag = any, State = GetItemsByTagsV2Request.Types.QuerySegmentState.Include });

        var reply = await _tagService.GetItemsByTagsV2Async(new GetItemsByTagsV2Request { QueryParams = { query } });

        var results = reply.TaggedItems
            .Select(taggedItem
                => (TaggableItem)(taggedItem.ItemCase switch
                {
                    TaggedItem.ItemOneofCase.File
                        => new TaggableFile { Path = taggedItem.File.Path, Tags = taggedItem.Tags.MapToDomain().ToHashSet() },
                    TaggedItem.ItemOneofCase.Folder
                        => new TaggableFolder { Path = taggedItem.File.Path, Tags = taggedItem.Tags.MapToDomain().ToHashSet() },
                    _ => throw new ArgumentOutOfRangeException()
                }))
            .Select(taggableItem => new TaggableItemViewModel(_tagService) { TaggableItem = taggableItem, AreTagsVisible = true })
            .ToList();

        Files.Clear();
        Files.AddRange(results);
    }

    [RelayCommand]
    private void RemoveTag(object tag)
    {
        var tagIndex = EnteredTags.IndexOf(tag);
        EnteredTags.RemoveAt(tagIndex + 1);
        EnteredTags.RemoveAt(tagIndex);
    }

    [RelayCommand]
    private void RemoveLast()
    {
        var lastTag = EnteredTags.LastOrDefault(o => o.GetType() == typeof(Tag));

        if (lastTag is null) return;

        var lastTagIndex = EnteredTags.IndexOf(lastTag);

        EnteredTags.RemoveAt(lastTagIndex + 1);
        EnteredTags.RemoveAt(lastTagIndex);
    }

    public async Task<List<FileSystemInfo>> VerifyItemsToAdd(IEnumerable<FileSystemInfo> infos)
    {
        var alreadyTaggedItems = new List<FileSystemInfo>();
        foreach (var info in infos)
        {
            var request = info switch
            {
                DirectoryInfo dirInfo => new GetItemRequest { File = new FileDto { Path = dirInfo.FullName } },
                FileInfo fileInfo => new GetItemRequest { Folder = new FolderDto { Path = fileInfo.FullName } },
                _ => throw new ArgumentOutOfRangeException(nameof(infos))
            };

            var reply = await _tagService.GetItemAsync(request);

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
                DirectoryInfo dirInfo
                    => new TagItemRequest { Folder = new FolderDto { Path = dirInfo.FullName }, Tag = any },
                FileInfo fileInfo
                    => new TagItemRequest { File = new FileDto { Path = fileInfo.FullName }, Tag = any },
                _ => throw new UnreachableException()
            };

            var reply = await _tagService.TagItemAsync(tagRequest);

            if (input.MoveToInternalStorage && tagRequest.ItemCase is TagItemRequest.ItemOneofCase.File)
            {
                await MoveToInternalStorage(info, tagRequest.File);
            }

            switch (reply.ResultCase)
            {
                case TagItemReply.ResultOneofCase.TaggedItem:
                    break;
                case TagItemReply.ResultOneofCase.ErrorMessage:
                    _logger.LogWarning("Unable to add tag item {Item} with a tag {TagName} to", tagRequest.ItemCase, tagName);
                    continue;
                default:
                    throw new ArgumentOutOfRangeException(nameof(input));
            }
        }

        await CommitSearch();
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

    [RelayCommand]
    private void AddSpecialTag(NameSpecialTag tag)
    {
        SearchText = "";
        EnteredTags.Insert(EnteredTags.Count - 1, new NameSpecialTag { FileName = tag.FileName });
        EnteredTags.Insert(EnteredTags.Count - 1, new LogicalOperator());
    }

    // todo: rename method name to be more descriptive, like "AddTagToSearchField" and check command usages
    [RelayCommand]
    private void AddTag()
    {
        var itemToAdd = SelectedItemFromPopup;

        if (itemToAdd is null || EnteredTags.Contains(itemToAdd)) return;

        if (SearchText.StartsWith('!'))
        {
            var excludeItemToAdd = itemToAdd with { Name = itemToAdd.Name?.Insert(0, "exclude:") };
            EnteredTags.Insert(EnteredTags.Count - 1, excludeItemToAdd);
        }
        else
        {
            EnteredTags.Insert(EnteredTags.Count - 1, itemToAdd);
        }

        EnteredTags.Insert(EnteredTags.Count - 1, new LogicalOperator());

        SearchText = "";
        SearchResults.Remove(itemToAdd);
    }

    public bool IsActive => _bassAudioCaptureService?.IsActive() ?? false;

    [RelayCommand]
    private async Task RecordAudio(bool isChecked)
    {
        if (isChecked)
        {
            StartRecord();
        }
        else
        {
            StopRecord();
            // var words = await _speechToTagSearchService.GetTags(_bassAudioCaptureService?.OutputFilePath);
            var words = await _speechToTagSearchService.GetTranscriptionWords(_bassAudioCaptureService?.OutputFilePath);

            var searchTags = words
                .Where(tagName => _tagService.DoesTagExists(new DoesTagExistsRequest { Tag = Any.Pack(new NormalTag { Name = tagName }) }).Exists)
                .Select(tagName => new Tag(tagName))
                .Where(tag => !EnteredTags.Contains(tag))
                .ToArray();

            if (searchTags.Length == 0) return;

            EnteredTags.Clear();
            EnteredTags.Add("");

            foreach (var tag in searchTags)
            {
                EnteredTags.Insert(EnteredTags.Count - 1, tag);
                EnteredTags.Insert(EnteredTags.Count - 1, new LogicalOperator());
            }
        }
    }

    private BassAudioCaptureService? _bassAudioCaptureService;

    [ObservableProperty]
    private double _voiceIntensity;

    public double MinVoiceIntensity { get; } = -90;
    public double MaxVoiceIntensity { get; } = -20;

    private void StartRecord()
    {
        _bassAudioCaptureService = new BassAudioCaptureService(3);
        _bassAudioCaptureService.Start();

        DispatcherTimer.Run(
            () =>
            {
                VoiceIntensity = _bassAudioCaptureService.GetVoiceIntensity();
                return IsActive;
            },
            TimeSpan.FromMilliseconds(100));

        OnPropertyChanged(nameof(IsActive));
    }

    private void StopRecord()
    {
        _bassAudioCaptureService?.Stop();
        _bassAudioCaptureService?.Dispose();

        OnPropertyChanged(nameof(IsActive));
    }

    private CancellationTokenSource? _cts;

    partial void OnSearchTextChanged(string? value)
    {
        if (value?.StartsWith('.') ?? false) // special tag
        {
            AreSpecialTagsVisible = true;
            return;
        }

        AreSpecialTagsVisible = false;

        _cts?.Cancel(false);
        _cts?.Dispose();
        _cts = new CancellationTokenSource();

        Dispatcher.UIThread.InvokeAsync(async () => await DoSearch(value?.TrimStart('!'), _cts.Token), DispatcherPriority.MaxValue);
    }

    private async Task DoSearch(string? value, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(value)) return;

        SearchResults.Clear(); // todo: it breaks without throttle

        var callOptions = new CallOptions().WithCancellationToken(ct);

        using var streamingCall = _tagService.SearchTags(
            new SearchTagsRequest { SearchText = value, SearchType = SearchTagsRequest.Types.SearchType.Fuzzy, ResultsLimit = 15 },
            callOptions);

        try
        {
            while (await streamingCall.ResponseStream.MoveNext(ct))
            {
                var reply = streamingCall.ResponseStream.Current;

                var highlightInfos = reply.MatchedPart
                    .Select(match => new HighlightInfo(match.StartIndex, match.Length))
                    .ToArray();

                var _ = _wordHighlighter.CreateInlines(TagMapper.MapToDomain(reply.Tag).DisplayText, highlightInfos);

                // SearchResults.Add(new Tag(reply.TagName, inlines));
            }
        }
        catch (RpcException e) when (e.Status.StatusCode == StatusCode.Cancelled)
        {
            // this.Log().Debug("Streaming of tag names hints for SearchBar was cancelled");
        }

        // does it has to be here? it is here because updating results reset selection of the list 
        SelectedItemFromSearched = SearchResults.FirstOrDefault();
    }

    public void Dispose()
    {
        _cts?.Dispose();
        GC.SuppressFinalize(this);
    }
}
