﻿using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls.Documents;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Grpc.Core;
using JetBrains.Annotations;
using TagTool.App.Core.Models;
using TagTool.App.Core.Services;
using TagTool.App.Extensions;
using TagTool.Backend;

namespace TagTool.App.ViewModels.UserControls;

public partial class TagSearchBoxViewModel : ViewModelBase, IDisposable
{
    private readonly TagSearchService.TagSearchServiceClient _tagSearchServiceClient;

    [ObservableProperty]
    private string? _searchText;

    public ObservableCollection<HighlightedMatch> TagsSearchResults { get; set; } = new();

    public TagSearchBoxViewModel() : this(Application.Current?.CreateInstance<TagSearchServiceFactory>()!)
    {
    }

    [UsedImplicitly]
    public TagSearchBoxViewModel(TagSearchServiceFactory tagSearchServiceFactory)
    {
        _tagSearchServiceClient = tagSearchServiceFactory.Create();

        TagsSearchResults.Add(new HighlightedMatch { MatchedText = "someMatch" });
        TagsSearchResults.Add(new HighlightedMatch { MatchedText = "someMatch" });
        // TagsSearchResults.Add("someMatch");
        // TagsSearchResults.Add("someMatch");
    }

    private CancellationTokenSource? _cts;

    [RelayCommand]
    public async Task DoSearchRelay()
    {
        await DoSearch(SearchText);
    }

    public async Task DoSearch(string? value)
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = new CancellationTokenSource();

        // TagsSearchResults.Clear(); // todo: it breaks without throttle
        if (string.IsNullOrEmpty(value)) return;

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
                    MatchedText = reply.MatchedTagName, Score = reply.Score //Inlines = FindSpans(reply.MatchedTagName, highlightInfos)
                };

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