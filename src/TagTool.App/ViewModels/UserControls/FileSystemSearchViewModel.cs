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
using TagTool.App.Core.Services;
using TagTool.Backend;

namespace TagTool.App.ViewModels.UserControls;

public partial class FileSystemSearchViewModel : Document, IDisposable
{
    private readonly SearchService.SearchServiceClient _fileSystemSearchService;

    [ObservableProperty]
    private string _searchRoot = @"C:\Users\tczyz\MyFiles\";

    [ObservableProperty]
    private string? _currentlySearchDir;

    [ObservableProperty]
    private bool _isSearching;

    public ObservableCollection<string> SearchResults { get; set; } = new();

    /// <summary>
    ///     ctor for XAML previewer
    /// </summary>
    public FileSystemSearchViewModel()
    {
        if (!Design.IsDesignMode)
        {
            Debug.Fail("ctor for XAML Previewer should not be invoke during standard execution");
        }

        _fileSystemSearchService = App.Current.Services.GetRequiredService<ITagToolBackend>().GetFileSystemSearchService();
    }

    [UsedImplicitly]
    public FileSystemSearchViewModel(ITagToolBackend tagToolBackend)
    {
        _fileSystemSearchService = tagToolBackend.GetFileSystemSearchService();
    }

    private AsyncDuplexStreamingCall<SearchRequest, SearchReply>? _streamingCall;
    private CancellationTokenSource? _cts;
    private string? _currentlySearchDirBuffer;

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
            Root = _searchRoot,
            Wildcard = new WildcardExpression { Pattern = "c02791*.svgz" },
            IgnoreCase = true,
            ExcludedPaths = { @"C:\Users\tczyz\MyFiles\Documents", @"C:\Users\tczyz\MyFiles\Andrzej" }
        };

        IsSearching = true;
        await _streamingCall.RequestStream.WriteAsync(searchRequest);

        Dispatcher.UIThread.Post(async () =>
        {
            // cancellationToken???
            await foreach (var reply in _streamingCall.ResponseStream.ReadAllAsync().WithCancellation(_cts.Token))
            {
                switch (reply.ContentCase)
                {
                    case SearchReply.ContentOneofCase.FullName:
                        SearchResults.Add(reply.FullName);
                        break;
                    case SearchReply.ContentOneofCase.CurrentlySearchDir:
                        _currentlySearchDirBuffer = reply.CurrentlySearchDir;
                        break;
                }
            }

            IsSearching = false;
        });
    }

    [RelayCommand]
    private async Task AddExcludedPath(string fullPath)
    {
        if (string.IsNullOrEmpty(fullPath)) return;

        if (IsSearching)
        {
            await _streamingCall.RequestStream.WriteAsync(new SearchRequest { ExcludedPaths = { fullPath } });
        }
    }

    partial void OnIsSearchingChanged(bool value)
    {
        if (value)
        {
            DispatcherTimer.Run(
                () =>
                {
                    if (IsSearching)
                    {
                        CurrentlySearchDir = _currentlySearchDirBuffer;
                    }

                    return IsSearching;
                },
                TimeSpan.FromMilliseconds(250),
                DispatcherPriority.Normal);
        }
        else
        {
            CurrentlySearchDir = null;
        }
    }

    public void Dispose()
    {
        _cts?.Dispose();
        GC.SuppressFinalize(this);
    }
}
