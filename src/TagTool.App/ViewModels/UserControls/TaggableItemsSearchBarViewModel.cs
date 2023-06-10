using System.Collections.ObjectModel;
using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DynamicData;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TagTool.App.Models;
using TagTool.App.Services;
using TagTool.Backend;

namespace TagTool.App.ViewModels.UserControls;

public sealed class TextBoxMarker
{
}

public sealed record QuerySegment(bool Include, bool MustBePresent, ITag Tag);

public partial class TaggableItemsSearchBarViewModel : ViewModelBase, IDisposable
{
    private readonly ILogger<TaggableItemsSearchBarViewModel> _logger;
    private readonly TagService.TagServiceClient _tagService;
    private readonly ISpeechToTagSearchService _speechToTagSearchService;

    public ObservableCollection<object> DisplayedSearchBarElements { get; } = new() { new TextBoxMarker() };

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
        _logger = logger;
        _tagService = tagToolBackend.GetTagService();
        _speechToTagSearchService = speechToTagSearchService;

        Initialize();
    }

    private void Initialize()
    {
        // QuerySegments.CollectionChanged += async (_, _) => await Dispatcher.UIThread.InvokeAsync(CommitSearch);
        QuerySegments.CollectionChanged
            += (_, args) =>
            {
                if (args.OldItems is not null)
                {
                    DisplayedSearchBarElements.RemoveMany(args.OldItems.OfType<object>());
                }

                if (args.NewItems is not null)
                {
                    DisplayedSearchBarElements.AddOrInsertRange(args.NewItems.OfType<object>(), 0);
                }
            };

        QuerySegments.Add(new QuerySegment(true, false, new TextTag("Default")));
        QuerySegments.Add(new QuerySegment(true, false, new TextTag("Default2")));
    }

    [RelayCommand]
    private async Task CommitSearch()
    {
        var tagQueryParams = QuerySegments.Select(segment
            => new GetItemsByTagsV2Request.Types.TagQueryParam
            {
                Tag = TagMapper.TagMapper.MapToDto(segment.Tag), Include = segment.Include, MustBePresent = segment.MustBePresent
            });

        // todo: inform other component (which is responsible for displaying found items) that query has changed
        var _ = await _tagService.GetItemsByTagsV2Async(new GetItemsByTagsV2Request { QueryParams = { tagQueryParams } });
    }

    [RelayCommand]
    private void RemoveTag(object querySegment)
    {
        if (querySegment is not QuerySegment segment) return;

        QuerySegments.Remove(segment);
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

            // var searchTags = words
            //     .Where(tagName => _tagService.DoesTagExists(new DoesTagExistsRequest { Tag = Any.Pack(new NormalTag { Name = tagName }) }).Exists)
            //     .Select(tagName => new Tag(tagName))
            //     .Where(tag => !EnteredTags.Contains(tag))
            //     .ToArray();
            //
            // if (searchTags.Length == 0) return;
            //
            // EnteredTags.Clear();
            // EnteredTags.Add("");
            //
            // foreach (var tag in searchTags)
            // {
            //     EnteredTags.Insert(EnteredTags.Count - 1, tag);
            //     EnteredTags.Insert(EnteredTags.Count - 1, new LogicalOperator());
            // }
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
