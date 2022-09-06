using System.Collections.ObjectModel;
using System.Reactive.Linq;
using Avalonia.Media;
using Grpc.Core;
using ReactiveUI;
using Splat;
using TagTool.App.Controls;
using TagTool.App.Core.Models;
using TagTool.Backend;

namespace TagTool.App.ViewModels;

public class TabContentViewModel : ViewModelBase, IDisposable
{
    // public ObservableCollection<HighlightedMatch> TagsSearchResults { get; set; } = new();
    public ObservableCollection<StyledText> TagsSearchResults { get; set; } = new();

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
            .Throttle(TimeSpan.FromMilliseconds(400))
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(DoSearch!);

        var styledText = StyledText.Create();
        styledText.Append("StyledText", new ImageBrush());

        TagsSearchResults.Add(styledText);
        TagsSearchResults.Add(styledText);
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
                    Spans = FindSpans(reply.MatchedTagName, highlightInfos),
                    HighlightedText = HighlightText(reply.MatchedTagName, highlightInfos),
                    Score = reply.Score
                };

                // TagsSearchResults.Add(viewListItem);
            }
        }
        catch (RpcException e) when (e.Status.StatusCode == StatusCode.Cancelled)
        {
            this.Log().Debug("Streaming of tag names hints for SearchBar was cancelled");
        }
        finally
        {
            // todo: do not create new class... manage existing collection
            // TagsSearchResults = new ObservableCollection<StyledText>(TagsSearchResults.OrderByDescending(item => item.Score));
        }
    }

    private static List<FormattedTextStyleSpan> FindSpans(string tagName, IReadOnlyCollection<HighlightInfo> highlightInfos)
    {
        var spans = new List<FormattedTextStyleSpan>();

        var lastIndex = 0;
        var index = 0;

        void FlushNotHighlighted()
        {
            if (lastIndex == index) return;

            // formattedText.Spans.Add(new Span<> { Text = tagName[lastIndex..index] });
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

                // var span = new FormattedTextStyleSpan(index, highlightedPart.Length, new LinearGradientBrush());
                // spans.Add(span);

                index += highlightedPart.Length;
                lastIndex = index;
            }
        }

        FlushNotHighlighted();
        return spans;
    }

    private static FormattedText HighlightText(string tagName, IReadOnlyCollection<HighlightInfo> highlightInfos)
    {
        var spans = new List<FormattedTextStyleSpan>();

        var lastIndex = 0;
        var index = 0;

        void FlushNotHighlighted()
        {
            if (lastIndex == index) return;

            // formattedText.Spans.Add(new Span<> { Text = tagName[lastIndex..index] });
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

                // var span = new FormattedTextStyleSpan(index, highlightedPart.Length, new LinearGradientBrush());
                // spans.Add(span);

                index += highlightedPart.Length;
                lastIndex = index;
            }
        }

        FlushNotHighlighted();

        var formattedText = new FormattedText { Text = tagName, Spans = spans };
        return formattedText;
    }

    public void Dispose()
    {
        _cts?.Dispose();
        GC.SuppressFinalize(this);
    }
}
