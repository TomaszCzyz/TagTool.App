using System.Collections.ObjectModel;
using System.Reactive.Linq;
using Avalonia.Controls.Documents;
using Avalonia.Media;
using Grpc.Core;
using ReactiveUI;
using Splat;
using TagTool.App.Core.Models;
using TagTool.Backend;

namespace TagTool.App.ViewModels;

public class TabContentViewModel : ViewModelBase, IDisposable
{
    public ObservableCollection<HighlightedMatch> TagsSearchResults { get; set; } = new();

    private string? _searchText;

    public string? SearchText
    {
        get => _searchText;
        set => this.RaiseAndSetIfChanged(ref _searchText, value);
    }

    public TabContentViewModel()
    {
        _tagSearchServiceClient = Locator.Current.GetService<TagSearchService.TagSearchServiceClient>()!;

        this.WhenAnyValue(x => x.SearchText)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Throttle(TimeSpan.FromMilliseconds(100))
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(DoSearch!);
    }

    private readonly TagSearchService.TagSearchServiceClient _tagSearchServiceClient;
    private CancellationTokenSource? _cts;

    private async void DoSearch(string value)
    {
        TagsSearchResults.Clear();
        if (string.IsNullOrEmpty(value)) return;

        _cts?.Cancel();
        _cts?.Dispose();
        _cts = new CancellationTokenSource();

        var matchTagsRequest = new MatchTagsRequest { PartialTagName = value, MaxReturn = 50 };
        var callOptions = new CallOptions().WithCancellationToken(_cts.Token);

        using var streamingCall = _tagSearchServiceClient.MatchTags(matchTagsRequest, callOptions);

        try
        {
            while (await streamingCall.ResponseStream.MoveNext(_cts.Token))
            {
                var reply = streamingCall.ResponseStream.Current;

                var highlightInfos = reply.MatchedParts
                    .Select(match => new HighlightInfo(match.StartIndex, match.Length))
                    .ToArray();

                var viewListItem = new HighlightedMatch
                {
                    MatchedText = reply.MatchedTagName,
                    Inlines = FindSpans(reply.MatchedTagName, highlightInfos),
                    Score = reply.Score
                };

                TagsSearchResults.Add(viewListItem);
            }
        }
        catch (RpcException e) when (e.Status.StatusCode == StatusCode.Cancelled)
        {
            this.Log().Debug("Streaming of tag names hints for SearchBar was cancelled");
        }
        finally
        {
            // todo: do not create new class... manage existing collection
            // TagsSearchResults = new ObservableCollection<HighlightedMatch>(TagsSearchResults.OrderByDescending(item => item.Score));
        }
    }

    private static InlineCollection FindSpans(string tagName, IReadOnlyCollection<HighlightInfo> highlightInfos)
    {
        var inlines = new InlineCollection();

        var lastIndex = 0;
        var index = 0;

        void FlushNotHighlighted()
        {
            if (lastIndex == index) return;

            inlines.Add(new Run { Text = tagName[lastIndex..index] });
        }

        while (index < tagName.Length)
        {
            var highlightedPart = highlightInfos.FirstOrDefault(info => info.StartIndex == index);

            if (highlightedPart is null)
            {
                index++;
            }
            else
            {
                FlushNotHighlighted();

                var endIndex = index + highlightedPart.Length;
                inlines.Add(new Run { Text = tagName[index..endIndex], Background = Brushes.DarkSeaGreen });

                index = endIndex;
                lastIndex = index;
            }
        }

        FlushNotHighlighted();
        return inlines;
    }

    public void Dispose()
    {
        _cts?.Dispose();
        GC.SuppressFinalize(this);
    }
}
