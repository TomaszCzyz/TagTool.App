using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DynamicData;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TagTool.App.Lite.Models;
using TagTool.App.Lite.Services;
using TagTool.Backend;
using TagTool.Backend.DomainTypes;
using DayRangeTag = TagTool.App.Lite.Models.DayRangeTag;
using DayTag = TagTool.App.Lite.Models.DayTag;

namespace TagTool.App.Lite.ViewModels;

public sealed class TextBoxMarker
{
}

public enum QuerySegmentState
{
    Exclude = 0,
    Include = 1,
    MustBePresent = 2
}

[DebuggerDisplay("State: {State}, Tag: {Tag}")]
public sealed class QuerySegment
{
    public QuerySegmentState State { get; set; } = QuerySegmentState.Include;

    public required ITag Tag { get; init; }

    private bool Equals(QuerySegment other) => Tag.Equals(other.Tag);

    public override bool Equals(object? obj) => ReferenceEquals(this, obj) || obj is QuerySegment other && Equals(other);

    public override int GetHashCode() => Tag.GetHashCode();
}

public partial class TaggableItemsSearchBarViewModel : ViewModelBase, IDisposable
{
    private readonly ILogger<TaggableItemsSearchBarViewModel> _logger;
    private readonly TagService.TagServiceClient _tagService;
    private readonly ISpeechToTagSearchService _speechToTagSearchService;

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
        File.WriteAllText(@"C:\Users\tczyz\Documents\TagToolApp\FromBarInParamLessCtor.txt", "");
   

        if (!Design.IsDesignMode)
        {
            Debug.Fail("ctor for XAML Previewer should not be invoke during standard execution");
        }

        _logger = App.Current.Services.GetRequiredService<ILogger<TaggableItemsSearchBarViewModel>>();
        _tagService = App.Current.Services.GetRequiredService<ITagToolBackend>().GetTagService();
        _speechToTagSearchService = null!;

        Initialize();
    }

    [UsedImplicitly]
    public TaggableItemsSearchBarViewModel(
        ILogger<TaggableItemsSearchBarViewModel> logger,
        ITagToolBackend tagToolBackend,
        ISpeechToTagSearchService speechToTagSearchService)
    {
        File.WriteAllText(@"C:\Users\tczyz\Documents\TagToolApp\FromBarInParamCtor.txt", "");
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
                if (args.OldItems?.Count == 1
                    && args.NewItems?.Count == 1
                    && (args.OldItems[0] as QuerySegment)!.Equals(args.NewItems[0] as QuerySegment))
                {
                    DisplayedSearchBarElements!.Replace(args.OldItems[0], args.NewItems[0]);
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
            };

        QuerySegments.Add(new QuerySegment { Tag = new TextTag { Name = "Default" } });
        QuerySegments.Add(new QuerySegment { State = QuerySegmentState.Exclude, Tag = new TextTag { Name = "Tag2" } });
        QuerySegments.Add(new QuerySegment { State = QuerySegmentState.MustBePresent, Tag = new DayTag { DayOfWeek = DayOfWeek.Sunday } });
        QuerySegments.Add(new QuerySegment { Tag = new DayRangeTag { Begin = DayOfWeek.Monday, End = DayOfWeek.Thursday } });
    }

    public bool FilterAlreadyUsedTags(string? _, object? item)
        => item is string tagNameFromDropDown
           && tagNameFromDropDown != "Unknown TagType"
           && !QuerySegments.Select(segment => segment.Tag.DisplayText).Contains(tagNameFromDropDown);

    public async Task<IEnumerable<object>> GetTagsAsync(string? _, CancellationToken cancellationToken)
    {
        var streamingCall = _tagService.SearchTags(
            new SearchTagsRequest { Name = "*", SearchType = SearchTagsRequest.Types.SearchType.Wildcard, ResultsLimit = 50 },
            cancellationToken: cancellationToken);

        var results = await streamingCall.ResponseStream
            .ReadAllAsync(cancellationToken)
            .Select(reply => TagMapper.TagMapper.MapToDomain(reply.Tag))
            .Where(tag => !QuerySegments.Select(segment => segment.Tag).Contains(tag))
            .Select(tag => (object)tag.DisplayText)
            .ToListAsync(cancellationToken);

        return results;
    }

    [RelayCommand]
    private async Task CommitSearch()
    {
        var tagQueryParams = QuerySegments.Select(segment
            => new GetItemsByTagsV2Request.Types.TagQueryParam
            {
                Tag = TagMapper.TagMapper.MapToDto(segment.Tag),
                Include = segment.State == QuerySegmentState.Include,
                MustBePresent = segment.State == QuerySegmentState.MustBePresent
            });

        // todo: inform other component (which is responsible for displaying found items) that query has changed
        var _ = await _tagService.GetItemsByTagsV2Async(new GetItemsByTagsV2Request { QueryParams = { tagQueryParams } });
    }

    [RelayCommand]
    private void AddTagToSearchQuery(string tagName)
    {
        QuerySegments.Add(new QuerySegment { Tag = new TextTag { Name = tagName } });
    }

    [RelayCommand]
    private void RemoveTag(object querySegment)
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

    public void Dispose()
    {
        _bassAudioCaptureService?.Dispose();
        GC.SuppressFinalize(this);
    }
}
