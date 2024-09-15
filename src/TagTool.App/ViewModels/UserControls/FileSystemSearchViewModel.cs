using System.Collections.ObjectModel;
using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Dock.Model.Mvvm.Controls;
using Grpc.Core;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using TagTool.App.Core;
using TagTool.App.Core.Models;
using TagTool.App.Core.Services;
using TagTool.App.Core.ViewModels;
using TagTool.Backend;

namespace TagTool.App.ViewModels.UserControls;

public partial class FileSystemSearchViewModel : Document, IDisposable
{
    private readonly SearchService.SearchServiceClient _fileSystemSearchService;
    private readonly TagService.TagServiceClient _tagService;

    private AsyncDuplexStreamingCall<SearchRequest, SearchReply>? _streamingCall;
    private CancellationTokenSource? _cts;
    private string? _currentlySearchDirBuffer;

    [ObservableProperty]
    private string _searchRoot = @"C:\Users\tczyz\MyFiles\";

    [ObservableProperty]
    private string? _currentlySearchDir;

    [ObservableProperty]
    private string? _searchPhrase = "c02791*.svgz";

    [ObservableProperty]
    private bool? _searchType;

    [ObservableProperty]
    private bool _ignoreCase = true;

    public ObservableCollection<TaggableItemViewModel> SearchResults { get; set; } = [];

    /// <summary>
    ///     ctor for XAML previewer
    /// </summary>
    public FileSystemSearchViewModel()
    {
        if (!Design.IsDesignMode)
        {
            Debug.Fail("ctor for XAML Previewer should not be invoke during standard execution");
        }

        _fileSystemSearchService = AppTemplate.Current.Services.GetRequiredService<ITagToolBackend>().GetFileSystemSearchService();
        _tagService = AppTemplate.Current.Services.GetRequiredService<ITagToolBackend>().GetTagService();
    }

    [UsedImplicitly]
    public FileSystemSearchViewModel(ITagToolBackend tagToolBackend)
    {
        _fileSystemSearchService = tagToolBackend.GetFileSystemSearchService();
        _tagService = tagToolBackend.GetTagService();
    }

    public void Dispose()
    {
        _cts?.Dispose();
        GC.SuppressFinalize(this);
    }

    [RelayCommand]
    private async Task StartSearch()
    {
        _cts?.Cancel(false);
        _cts?.Dispose();
        _cts = new CancellationTokenSource();

        _streamingCall = _fileSystemSearchService.Search();

        var searchRequest = new SearchRequest
        {
            Depth = int.MaxValue,
            Root = SearchRoot,
            IgnoreCase = IgnoreCase
        };

        switch (SearchType)
        {
            case true:
                searchRequest.Exact = new ExactExpression { Substring = SearchPhrase };
                break;
            case null:
                searchRequest.Wildcard = new WildcardExpression { Pattern = SearchPhrase };
                break;
            case false:
                searchRequest.Regex = new RegexExpression { Pattern = SearchPhrase };
                break;
        }

        await _streamingCall.RequestStream.WriteAsync(searchRequest);

        RunCurrentlySearchDirUpdates();

        await foreach (var reply in _streamingCall.ResponseStream.ReadAllAsync().WithCancellation(_cts.Token))
        {
            switch (reply.ContentCase)
            {
                case SearchReply.ContentOneofCase.FullName:

                    TaggableItem taggableItem = Directory.Exists(reply.FullName)
                        ? new TaggableFolder { Path = reply.FullName }
                        : new TaggableFile { Path = reply.FullName };

                    var taggableItemViewModel = new TaggableItemViewModel(_tagService) { TaggableItem = taggableItem, AreTagsVisible = true };
                    SearchResults.Add(taggableItemViewModel);
                    break;
                case SearchReply.ContentOneofCase.CurrentlySearchDir:
                    _currentlySearchDirBuffer = reply.CurrentlySearchDir;
                    break;
            }
        }

        CurrentlySearchDir = null;
    }

    private void RunCurrentlySearchDirUpdates()
        => DispatcherTimer.Run(
            () =>
            {
                if (StartSearchCommand.IsRunning)
                {
                    CurrentlySearchDir = _currentlySearchDirBuffer;
                }

                return StartSearchCommand.IsRunning;
            },
            TimeSpan.FromMilliseconds(250),
            DispatcherPriority.Normal);

    [RelayCommand]
    private async Task AddExcludedPath(string fullPath)
    {
        if (string.IsNullOrEmpty(fullPath))
        {
            return;
        }

        if (StartSearchCommand.IsRunning) // not necessary as parameter of this command will always be empty string, when search is not running
        {
            await _streamingCall.RequestStream.WriteAsync(new SearchRequest { ExcludedPaths = { fullPath } });
        }
    }
}
