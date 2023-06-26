using System.Collections;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Avalonia.Controls;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DynamicData;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using TagTool.App.Core.Models;
using TagTool.App.Core.Services;
using TagTool.Backend;
using TagTool.Backend.DomainTypes;

namespace TagTool.App.Core.ViewModels;

public class CommitSearchQueryEventArgs : EventArgs
{
    public ICollection<QuerySegment> QuerySegments { get; init; }

    public CommitSearchQueryEventArgs(ICollection<QuerySegment> querySegments) => QuerySegments = querySegments;
}

public sealed partial class TaggableItemsSearchBarViewModel : ViewModelBase, IDisposable
{
    private readonly ILogger<TaggableItemsSearchBarViewModel> _logger;
    private readonly TagService.TagServiceClient _tagService;
    private readonly ISpeechToTagSearchService _speechToTagSearchService;

    private IList<ITag>? _tagsInDropDown;

    /// <summary>
    ///     The event raised when:
    ///     <list type="bullet">
    ///         <item>
    ///             <description>
    ///                 <see cref="TagTool.App.Core.ViewModels.TaggableItemsSearchBarViewModel.QuerySegments" />
    ///                 collection is updated
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>search icon is clicked</description>
    ///         </item>
    ///         <item>
    ///             <description>commit search shortcut is used</description>
    ///         </item>
    ///     </list>
    /// </summary>
    public event EventHandler<CommitSearchQueryEventArgs>? CommitSearchQueryEvent;

    public ObservableCollection<object> DisplayedSearchBarElements { get; } = new() { new TextBoxMarker() };

    [ObservableProperty]
    private object? _selectedItem;

    public ObservableCollection<QuerySegment> QuerySegments { get; } = new();

    [ObservableProperty]
    private string _searchText = "";

    /// <summary>
    ///     ctor for XAML previewer
    /// </summary>
    public TaggableItemsSearchBarViewModel()
    {
        if (!Design.IsDesignMode)
        {
            Debug.Fail("ctor for XAML Previewer should not be invoke during standard execution");
        }

        // _logger = App.Current.Services.GetRequiredService<ILogger<TaggableItemsSearchBarViewModel>>();
        // _tagService = App.Current.Services.GetRequiredService<ITagToolBackend>().GetTagService();
        _logger = null!;
        _tagService = null!;
        _speechToTagSearchService = null!;

        Initialize();
    }

    [UsedImplicitly]
    public TaggableItemsSearchBarViewModel(
        ILogger<TaggableItemsSearchBarViewModel> logger,
        ITagToolBackend tagToolBackend,
        ISpeechToTagSearchService speechToTagSearchService)
    {
        _logger = logger;
        _tagService = tagToolBackend.GetTagService();
        _speechToTagSearchService = speechToTagSearchService;

        Initialize();
    }

    private void Initialize()
    {
        QuerySegments.CollectionChanged
            += (_, args) =>
            {
                if (IsOneElementUpdated(args.OldItems, args.NewItems))
                {
                    DisplayedSearchBarElements!.Replace(args.OldItems[0], args.NewItems[0]);
                    OnCommitSearchQueryEvent(new CommitSearchQueryEventArgs(QuerySegments));
                    return;
                }

                if (args.OldItems is not null)
                {
                    DisplayedSearchBarElements.RemoveMany(args.OldItems.OfType<object>());
                }

                if (args.NewItems is not null)
                {
                    DisplayedSearchBarElements.AddOrInsertRange(args.NewItems.OfType<object>(), DisplayedSearchBarElements.Count - 1);
                }

                OnCommitSearchQueryEvent(new CommitSearchQueryEventArgs(QuerySegments));
            };
    }

    /// <summary>
    ///     Checks if the change refers to the same query segment (only its state has changed),
    ///     because then we should replace element to retain order.
    /// </summary>
    /// <returns>True, when lists contains only one element with the same tag</returns>
    private static bool IsOneElementUpdated([NotNullWhen(true)] IList? argsOldItems, [NotNullWhen(true)] IList? argsNewItems)
        => argsOldItems?.Count == 1 && argsNewItems?.Count == 1 && (argsOldItems[0] as QuerySegment)!.Equals(argsNewItems[0] as QuerySegment);

    public bool FilterAlreadyUsedTags(string? _, object? item)
        => item is string tagNameFromDropDown
           && tagNameFromDropDown != "Unknown TagType"
           && !QuerySegments.Select(segment => segment.Tag.DisplayText).Contains(tagNameFromDropDown);

    public async Task<IEnumerable<object>> GetTagsAsync(string? searchText, CancellationToken cancellationToken)
    {
        var streamingCall = _tagService.SearchTags(
            new SearchTagsRequest { SearchText = searchText, SearchType = SearchTagsRequest.Types.SearchType.Fuzzy, ResultsLimit = 30 },
            cancellationToken: cancellationToken);

        _tagsInDropDown = await streamingCall.ResponseStream
            .ReadAllAsync(cancellationToken)
            .Select(reply => TagMapper.TagMapper.MapToDomain(reply.Tag))
            .Where(tag => !QuerySegments.Select(segment => segment.Tag).Contains(tag))
            .ToListAsync(cancellationToken);

        return _tagsInDropDown.Select(tag => (object)tag.DisplayText);
    }

    [RelayCommand]
    private void AddTagToSearchQuery(string tagName)
    {
        var first = _tagsInDropDown?.First(tag => tag.DisplayText == tagName)!;
        QuerySegments.Add(new QuerySegment { Tag = first });
    }

    [RelayCommand]
    private void RemoveTagFromSearchQuery(object querySegment)
    {
        if (querySegment is not QuerySegment segment) return;

        QuerySegments.Remove(segment);
    }

    [RelayCommand]
    private void UpdateQuerySegmentState(object querySegment)
    {
        if (SelectedItem is not QuerySegment || querySegment is not QuerySegmentState newState) return;

        var indexOf = QuerySegments.IndexOf(SelectedItem);

        QuerySegments[indexOf] = new QuerySegment { Tag = QuerySegments[indexOf].Tag, State = newState };
    }

    [RelayCommand]
    private void CommitSearch()
    {
        OnCommitSearchQueryEvent(new CommitSearchQueryEventArgs(QuerySegments));
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
            var words = await _speechToTagSearchService.GetTranscriptionWords(_bassAudioCaptureService?.OutputFilePath);

            var searchTags = words
                .Select(tagName =>
                {
                    var tag = new NormalTag { Name = tagName };

                    var reply = _tagService.DoesTagExists(new DoesTagExistsRequest { Tag = Any.Pack(new NormalTag { Name = tagName }) });

                    return reply.Exists ? tag : null;
                })
                .Where(tag => tag is not null && !QuerySegments.Select(segment => segment.Tag.DisplayText).Contains(tag.Name))
                .Select(tag => TagMapper.TagMapper.MapToDomain(Any.Pack(tag)))
                .ToArray();

            if (searchTags.Length == 0) return;

            foreach (var tag in searchTags)
            {
                QuerySegments.Add(new QuerySegment { Tag = tag });
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

    private void OnCommitSearchQueryEvent(CommitSearchQueryEventArgs e)
    {
        Debug.WriteLine($"CommitSearchQueryEvent invoked with {string.Join(", ", e.QuerySegments.Select(segment => segment.Tag.DisplayText))}");
        CommitSearchQueryEvent?.Invoke(this, e);
    }

    public void Dispose()
    {
        _bassAudioCaptureService?.Dispose();
    }
}
