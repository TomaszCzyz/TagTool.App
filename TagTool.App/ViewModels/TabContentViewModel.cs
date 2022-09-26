using System.Collections.ObjectModel;
using System.Reactive.Linq;
using Avalonia.Controls.Documents;
using Avalonia.Media;
using Dock.Model.ReactiveUI.Controls;
using DynamicData;
using Grpc.Core;
using ReactiveUI;
using Splat;
using TagTool.App.Core.Models;
using TagTool.Backend;
using File = TagTool.App.Models.File;

namespace TagTool.App.ViewModels;

public class TabContentViewModel : Document, IDisposable
{
    public ObservableCollection<File> Files { get; set; } = new();

    public ObservableCollection<HighlightedMatch> TagsSearchResults { get; set; } = new();

    public ObservableCollection<string> EnteredTags { get; set; } = new();

    private string? _searchText;

    public string? SearchText
    {
        get => _searchText;
        set => this.RaiseAndSetIfChanged(ref _searchText, value);
    }

    public TabContentViewModel()
    {
        Files.AddRange(_exampleFiles);

        _tagSearchServiceClient = Locator.Current.GetService<TagSearchService.TagSearchServiceClient>()!;

        this.WhenAnyValue(x => x.SearchText)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Throttle(TimeSpan.FromMilliseconds(100))
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(DoSearch!);

        EnteredTags.AddRange(new[] { "Tag1", "Audio", "Dog", "Picture", "Colleague", "Tag6" });
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

    private readonly File[] _exampleFiles =
    {
        new(1, "File1", 1234, new DateTime(2022, 12, 12), null, @"C:\Program Files"),
        new(1, "File1", 1234, new DateTime(2022, 12, 12), null, @"C:\Program Files"),
        new(1, "File1", 1234, new DateTime(2022, 12, 12), null, @"C:\Program Files"),
        new(1, "File1", 1234, new DateTime(2022, 12, 12), null, @"C:\Program Files"),
        new(1, "File1", 1234, new DateTime(2022, 12, 12), null, @"C:\Program Files"),
        new(1, "File1", 1234, new DateTime(2022, 12, 12), null, @"C:\Program Files"),
        new(1, "File1", 1234, new DateTime(2022, 12, 12), null, @"C:\Program Files"),
        new(1, "File1", 1234, new DateTime(2022, 12, 12), null, @"C:\Program Files"),
        new(1, "File1", 1234, new DateTime(2022, 12, 12), null, @"C:\Program Files"),
        new(1, "File1", 1234, new DateTime(2022, 12, 12), null, @"C:\Program Files"),
        new(1, "File1", 1234, new DateTime(2022, 12, 12), null, @"C:\Program Files"),
        new(1, "File1", 1234, new DateTime(2022, 12, 12), null, @"C:\Program Files"),
        new(2, "File2", 1234, new DateTime(1999, 1, 1), null, @"C:\Users\tczyz\Source\repos\LayersTraversing"),
        new(3, "File3", 144234, new DateTime(2022, 2, 12), null, @"C:\Program Files"),
        new(4, "FileFile4", 13234, new DateTime(202, 12, 30), null, @"C:\Users\tczyz\Source"),
        new(5, "File5", 122334, new DateTime(1990, 12, 30), null, @"C:\Users\tczyz\Source\repos\LayersTraversing\file.txt")
    };
}
