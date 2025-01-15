using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DynamicData;
using Grpc.Core;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using TagTool.App.Core.Models;
using TagTool.App.Core.Services;
using TagTool.BackendNew;

namespace TagTool.App.Core.ViewModels;

public class CommitSearchQueryEventArgs : EventArgs
{
    public ICollection<QuerySegment> QuerySegments { get; init; }

    public CommitSearchQueryEventArgs(ICollection<QuerySegment> querySegments)
    {
        QuerySegments = querySegments;
    }
}

public sealed partial class TaggableItemsSearchBarViewModel : ViewModelBase
{
    private readonly ILogger<TaggableItemsSearchBarViewModel> _logger;
    private readonly TagService.TagServiceClient _tagService;
    private IList<ITag>? _tagsInDropDown;

    [ObservableProperty]
    private object? _selectedItem;

    [ObservableProperty]
    private string _searchText = "";

    [ObservableProperty]
    private double _voiceIntensity;

    public ObservableCollection<object> DisplayedSearchBarElements { get; } = [new TextBoxMarker()];

    public ObservableCollection<QuerySegment> QuerySegments { get; } = [];

    public double MinVoiceIntensity { get; } = -90;

    public double MaxVoiceIntensity { get; } = -20;

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

        Initialize();
    }

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

    private void Initialize() => QuerySegments.CollectionChanged += UpdateDisplayedSearchBarElements;

    private void UpdateDisplayedSearchBarElements(object? sender, NotifyCollectionChangedEventArgs args)
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
           && !QuerySegments.Select(segment => segment.Tag.Text).Contains(tagNameFromDropDown);

    public async Task<IEnumerable<object>> GetTagsAsync(string? searchText, CancellationToken cancellationToken)
    {
        var callOptions = new CallOptions()
            .WithCancellationToken(cancellationToken)
            .WithDeadline(DateTime.UtcNow.AddMinutes(1));

        var streamingCall = _tagService.SearchTags(
            new SearchTagsRequest
            {
                SearchText = searchText,
                SearchType = SearchTagsRequest.Types.SearchType.Fuzzy,
                ResultsLimit = 30
            },
            callOptions);

        _tagsInDropDown = await streamingCall.ResponseStream
            .ReadAllAsync(cancellationToken)
            // .Select(reply => TagMapper.TagMapper.MapToDomain(reply.Tag))
            .Where(tag => !QuerySegments.Select(segment => segment.Tag).Contains(tag))
            .ToListAsync(cancellationToken);

        return _tagsInDropDown.Select(object (tag) => tag.DisplayText);
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
        if (querySegment is not QuerySegment segment)
        {
            return;
        }

        QuerySegments.Remove(segment);
    }

    [RelayCommand]
    private void UpdateQuerySegmentState(object querySegment)
    {
        if (SelectedItem is not QuerySegment || querySegment is not QuerySegmentState newState)
        {
            return;
        }

        var indexOf = QuerySegments.IndexOf(SelectedItem);

        QuerySegments[indexOf] = new QuerySegment { Tag = QuerySegments[indexOf].Tag, State = newState };
    }

    [RelayCommand]
    private void CommitSearch() => OnCommitSearchQueryEvent(new CommitSearchQueryEventArgs(QuerySegments));

    [RelayCommand]
    private async Task RecordAudio(bool isChecked)
    {
    }

    private void OnCommitSearchQueryEvent(CommitSearchQueryEventArgs e)
    {
        _logger.LogDebug("CommitSearchQueryEvent invoked with {@QuerySegments}", e.QuerySegments);
        CommitSearchQueryEvent?.Invoke(this, e);
    }
}
