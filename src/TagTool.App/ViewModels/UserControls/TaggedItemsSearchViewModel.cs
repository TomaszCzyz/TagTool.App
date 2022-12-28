﻿using System.Collections.ObjectModel;
using Avalonia.Controls.Documents;
using Avalonia.Media;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DynamicData;
using Grpc.Core;
using JetBrains.Annotations;
using TagTool.App.Core.Models;
using TagTool.App.Core.Services;
using TagTool.Backend;

namespace TagTool.App.ViewModels.UserControls;

public partial class TaggedItemsSearchViewModel : ViewModelBase, IDisposable
{
    private readonly TagSearchService.TagSearchServiceClient _tagSearchService;
    private readonly TagService.TagServiceClient _tagService;

    [ObservableProperty]
    private string? _searchText;

    [ObservableProperty]
    private Tag? _selectedItemFromSearched;

    [ObservableProperty]
    private Tag? _selectedItemFromPopular;

    public ObservableCollection<Tag> SearchResults { get; set; } = new();

    public ObservableCollection<Tag> PopularTags { get; set; } = new();

    public ObservableCollection<object> EnteredTags { get; set; } = new();

    public ObservableCollection<TaggedItem> Files { get; set; } = new();

    private IEnumerable<Tag> Tags => EnteredTags.Where(o => o.GetType() == typeof(Tag)).Cast<Tag>().ToArray();

    /// <summary>
    ///     ctor for XAML previewer
    /// </summary>
    public TaggedItemsSearchViewModel()
    {
        _tagSearchService = null!;
        _tagService = null!;
    }

    [UsedImplicitly]
    public TaggedItemsSearchViewModel(ITagToolBackend tagToolBackend)
    {
        _tagSearchService = tagToolBackend.GetSearchService();
        _tagService = tagToolBackend.GetTagService();

        var tags = new Tag[] { new("Audio"), new("Dog"), new("Picture"), new("Colleague"), new("Tag6"), new("LastTag") };
        EnteredTags.AddRange(tags);

        var popularTags = new Tag[] { new("SomeTag"), new("Tag"), new("SomeTag"), new("Picture"), new("Tag"), new("Picture"), new("Picture") };
        PopularTags.AddRange(popularTags);

        var searchResults = new Tag[] { new("Result1"), new("Tag"), new("Result2"), new("Picture"), new("SearchTag") };
        SearchResults.AddRange(searchResults);

        EnteredTags.Add("");
        Files.AddRange(_exampleFiles);
    }

    [RelayCommand]
    private async Task CommitSearch()
    {
        Files.Clear();

        var getItemsRequest = new GetItemsRequest { TagNames = { Tags.Select(tag => tag.Name).ToArray() } };
        var streamingCall = _tagService.GetItems(getItemsRequest);
        while (await streamingCall.ResponseStream.MoveNext())
        {
            var reply = streamingCall.ResponseStream.Current;
            if (reply.FileInfo is not null)
            {
                var info = new FileInfo(reply.FileInfo.Path);
                Files.Add(new TaggedItem(info.Name, info.Length, info.CreationTime, info.LastWriteTime, new[] { new Tag("asd"), new Tag("ert") }));
            }
            else
            {
                var info = new DirectoryInfo(reply.FolderInfo.Path);
                Files.Add(new TaggedItem(info.Name, 0, info.CreationTime, info.LastWriteTime, new[] { new Tag("asd"), new Tag("ert") }));
            }
        }
    }

    [RelayCommand]
    private void RemoveTag(Tag tag)
    {
        EnteredTags.Remove(tag);
    }

    [RelayCommand]
    private void RemoveLast()
    {
        var lastTag = EnteredTags.LastOrDefault(o => o.GetType() == typeof(Tag));

        if (lastTag is null) return;

        EnteredTags.Remove(lastTag);
    }

    [RelayCommand]
    private void UpdateSearch()
    {
        SelectedItemFromSearched = SearchResults.FirstOrDefault();
    }

    [RelayCommand]
    private void AddTag()
    {
        var itemToAdd = SelectedItemFromSearched ?? SelectedItemFromPopular;

        if (itemToAdd is null || EnteredTags.Contains(itemToAdd)) return;

        SearchText = "";
        SearchResults.Remove(itemToAdd);

        EnteredTags.Insert(EnteredTags.Count - 1, itemToAdd);
    }

    private CancellationTokenSource? _cts;

    partial void OnSearchTextChanged(string? value)
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

        SearchResults.Clear(); // todo: it breaks without throttle

        var matchTagsRequest = new MatchTagsRequest { PartialTagName = value, MaxReturn = 50 };
        var callOptions = new CallOptions().WithCancellationToken(ct);

        using var streamingCall = _tagSearchService.MatchTags(matchTagsRequest, callOptions);

        try
        {
            while (await streamingCall.ResponseStream.MoveNext(ct))
            {
                var reply = streamingCall.ResponseStream.Current;

                var highlightInfos = reply.MatchedParts
                    .Select(match => new HighlightInfo(match.StartIndex, match.Length))
                    .ToArray();

                var inlines = FindSpans(reply.MatchedTagName, highlightInfos);

                SearchResults.Add(new Tag(reply.MatchedTagName, inlines));
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

                var solidColorBrush = new SolidColorBrush(Color.Parse("#0F7EBD"));
                var run = new Run
                {
                    Text = tagName[index..endIndex],
                    TextDecorations = new TextDecorationCollection { new() { Location = TextDecorationLocation.Underline, Stroke = solidColorBrush } }
                    // FontWeight = FontWeight.Bold,
                    // Foreground = new SolidColorBrush(Color.Parse("#EEEEEE"));
                    // Background = solidColorBrush;
                };
                inlines.Add(run);

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

    private readonly TaggedItem[] _exampleFiles =
    {
        new("File1.txt", 1234, new DateTime(2022, 12, 12), new DateTime(2022, 12, 12), new[] { new Tag("empty") }),
        new("File2.txt", 12311111114, new DateTime(2022, 12, 12), new DateTime(2022, 12, 12), new[] { new Tag("empty") }),
        new("File3.txt", 1234, new DateTime(2022, 12, 12), new DateTime(2022, 12, 12), new[] { new Tag("empty") }),
        new("File4.txt", 1212312334, new DateTime(2022, 12, 12), new DateTime(2022, 12, 12), new[] { new Tag("empty") }),
        new("File5.txt", 1234, new DateTime(2022, 12, 12), new DateTime(2022, 12, 12), new[] { new Tag("empty") }),
        new("File6.txt", 1222234, new DateTime(2022, 12, 12), new DateTime(2022, 12, 12), new[] { new Tag("empty") }),
        new("File1.txt", 1234, new DateTime(2022, 12, 12), new DateTime(2022, 12, 12), new[] { new Tag("empty") }),
        new("File1.txt", 1234, new DateTime(2022, 12, 12), new DateTime(2022, 12, 12), new[] { new Tag("empty") }),
        new("File1.txt", 1234, new DateTime(2022, 12, 12), new DateTime(2022, 12, 12), new[] { new Tag("empty") }),
        new("File1.txt", 1212334, new DateTime(2022, 12, 12), new DateTime(2022, 12, 12), new[] { new Tag("empty") }),
        new("File1.txt", 1234, new DateTime(2022, 12, 12), new DateTime(2022, 12, 12), new[] { new Tag("empty") }),
        new("File1.txt", 1234, new DateTime(2022, 12, 12), new DateTime(2022, 12, 12), new[] { new Tag("empty") }),
        new("File2.txt", 1234, new DateTime(1999, 1, 1), new DateTime(1999, 1, 1), new[] { new Tag("empty") }),
        new("File3.txt", 144234, new DateTime(2022, 2, 12), null, new[] { new Tag("empty") }),
        new("FileFile4", 13234, new DateTime(202, 12, 30), null, new[] { new Tag("empty") }),
        new("File5", 122334, new DateTime(1990, 12, 30), null, new[] { new Tag("empty") })
    };
}
