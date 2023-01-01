﻿using System.Collections.ObjectModel;
using System.Globalization;
using Avalonia.Controls.Documents;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DynamicData;
using TagTool.App.Core.Models;
using TagTool.Backend;

namespace TagTool.App.ViewModels.UserControls;

public enum TaggedItemType
{
    File = 0,
    Folder = 1
}

public partial class TaggableItemViewModel : ViewModelBase, IFileSystemEntry
{
    private readonly TagService.TagServiceClient _tagService;

    public TaggedItemType TaggedItemType { get; init; }

    [ObservableProperty]
    private string _displayName = "";

    [ObservableProperty]
    private string _location = "";

    [ObservableProperty]
    private DateTime _dateCreated;

    [ObservableProperty]
    private long? _size;

    [ObservableProperty]
    private InlineCollection _inlines = new();

    [ObservableProperty]
    private bool _areTagsVisible = true;

    public ObservableCollection<Tag> AssociatedTags { get; } = new();

    public TaggableItemViewModel(TagService.TagServiceClient tagServiceClient)
    {
        _tagService = tagServiceClient;
        var _ = Dispatcher.UIThread.InvokeAsync(UpdateTags);
    }

    [RelayCommand]
    private async Task TagIt(string tagName)
    {
        var tagRequest = TaggedItemType == TaggedItemType.Folder
            ? new TagRequest { TagNames = { tagName }, FolderInfo = new FolderDescription { Path = Location } }
            : new TagRequest { TagNames = { tagName }, FileInfo = new FileDescription { Path = Location } };

        var streamingCall = _tagService.Tag();

        await streamingCall.RequestStream.WriteAsync(tagRequest);
        await streamingCall.RequestStream.CompleteAsync();

        // todo: add only if success
        AssociatedTags.Add(new Tag(tagName));
    }

    [RelayCommand]
    private async Task UntagItem(string tagName)
    {
        var untagRequest = TaggedItemType == TaggedItemType.Folder
            ? new UntagRequest { TagNames = { tagName }, FolderInfo = new FolderDescription { Path = Location } }
            : new UntagRequest { TagNames = { tagName }, FileInfo = new FileDescription { Path = Location } };

        var streamingCall = _tagService.Untag();

        await streamingCall.RequestStream.WriteAsync(untagRequest);
        await streamingCall.RequestStream.CompleteAsync();

        // todo: remove only if success
        AssociatedTags.Remove(new Tag(tagName));
    }

    private void UpdateTags()
    {
        var getItemInfoRequest = new GetItemInfoRequest
        {
            Type = TaggedItemType.ToString().ToLower(CultureInfo.InvariantCulture), ItemIdentifier = Location
        };

        var getItemInfoReply = _tagService.GetItemInfo(getItemInfoRequest);

        // todo: change to AddIfNotExists
        AssociatedTags.Clear();
        AssociatedTags.AddRange(getItemInfoReply.Tags.Select(s => new Tag(s)).ToList());
    }
}