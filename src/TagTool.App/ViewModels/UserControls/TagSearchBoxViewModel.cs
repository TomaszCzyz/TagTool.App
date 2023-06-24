using System.Collections.ObjectModel;
using Avalonia.Controls.Documents;
using Avalonia.Media;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Grpc.Core;
using JetBrains.Annotations;
using TagTool.App.Core.Models;
using TagTool.App.Core.Services;
using TagTool.App.Core.TagMapper;
using TagTool.App.Core.ViewModels;
using TagTool.App.Models;
using TagTool.Backend;

namespace TagTool.App.ViewModels.UserControls;

public partial class TagSearchBoxViewModel : ViewModelBase, IDisposable
{
    private readonly TagService.TagServiceClient _tagService;

    public ITagsContainer? TagsContainer { get; set; }

    [ObservableProperty]
    private string? _text;

    [ObservableProperty]
    private HighlightedMatch? _selectedItem;

    public ObservableCollection<HighlightedMatch> TagsSearchResults { get; set; } = new();

    /// <summary>
    ///     ctor for XAML previewer
    /// </summary>
    [UsedImplicitly]
    public TagSearchBoxViewModel()
    {
        _tagService = null!;

        TagsSearchResults.Add(new HighlightedMatch { MatchedText = "someMatch" });
        TagsSearchResults.Add(new HighlightedMatch { MatchedText = "NewTag" });
        TagsSearchResults.Add(new HighlightedMatch { MatchedText = "someOtherMatch" });
    }

    public TagSearchBoxViewModel(ITagToolBackend tagToolBackend)
    {
        _tagService = tagToolBackend.GetTagService();
        TagsSearchResults.Add(new HighlightedMatch { MatchedText = "NewTag" });
        TagsSearchResults.Add(new HighlightedMatch { MatchedText = "someOtherMatch" });
        TagsSearchResults.Add(new HighlightedMatch { MatchedText = "someMatch" });
    }

    [RelayCommand]
    private void CommitTag()
    {
        if (SelectedItem is null) return;

        TagsContainer?.AddTag(new Tag(SelectedItem.MatchedText));
        Text = "";
    }

    [RelayCommand]
    private void RemoveTag()
    {
        TagsContainer?.RemoveLast();
    }

    private CancellationTokenSource? _cts;

    partial void OnTextChanged(string? value)
    {
        _cts?.Cancel(false);
        _cts?.Dispose();
        _cts = new CancellationTokenSource();

        async void Action() => await DoSearch(value, _cts.Token);

        Dispatcher.UIThread.InvokeAsync(Action, DispatcherPriority.MaxValue);
    }

    private async Task DoSearch(string? value, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(value)) return;

        TagsSearchResults.Clear(); // todo: it breaks without throttle

        var searchTagsRequest = new SearchTagsRequest { SearchType = SearchTagsRequest.Types.SearchType.Fuzzy, SearchText = value, ResultsLimit = 50 };
        var callOptions = new CallOptions().WithCancellationToken(ct);

        using var streamingCall = _tagService.SearchTags(searchTagsRequest, callOptions);

        try
        {
            while (await streamingCall.ResponseStream.MoveNext(ct))
            {
                var reply = streamingCall.ResponseStream.Current;

                var highlightInfos = reply.MatchedPart
                    .Select(match => new HighlightInfo(match.StartIndex, match.Length))
                    .ToArray();

                var viewListItem
                    = new HighlightedMatch { Inlines = FindSpans(TagMapper.MapToDomain(reply.Tag).DisplayText, highlightInfos) };

                TagsSearchResults.Add(viewListItem);
            }
        }
        catch (RpcException e) when (e.Status.StatusCode == StatusCode.Cancelled)
        {
            // this.Log().Debug("Streaming of tag names hints for SearchBar was cancelled");
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

                var solidColorBrush = new SolidColorBrush(Color.FromRgb(35, 85, 177), 0.6);
                inlines.Add(new Run { Text = tagName[index..endIndex], FontWeight = FontWeight.Bold, Background = solidColorBrush });

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
