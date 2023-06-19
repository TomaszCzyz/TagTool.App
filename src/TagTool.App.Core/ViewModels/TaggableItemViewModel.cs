using System.Collections.ObjectModel;
using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DynamicData;
using Google.Protobuf.WellKnownTypes;
using TagTool.App.Core.Models;
using TagTool.Backend;
using TagTool.Backend.DomainTypes;

namespace TagTool.App.Core.ViewModels;

public enum TaggedItemType
{
    File = 0,
    Folder = 1
}

[DebuggerDisplay("{DisplayName}")]
public partial class TaggableItemViewModel : ViewModelBase
{
    private readonly TagService.TagServiceClient _tagService;

    public TaggedItemType TaggedItemType { get; init; }

    [ObservableProperty]
    private string _displayName = "";

    [ObservableProperty]
    private InlineCollection _inlines = new();

    [ObservableProperty]
    private string _location = "";

    [ObservableProperty]
    private DateTime _dateCreated;

    [ObservableProperty]
    private long? _size;

    [ObservableProperty]
    private bool _areTagsVisible = true;

    public ObservableCollection<Tag> AssociatedTags { get; } = new();

    /// <summary>
    ///     ctor for XAML previewer
    /// </summary>
    public TaggableItemViewModel()
    {
        if (!Design.IsDesignMode)
        {
            Debug.Fail("ctor for XAML Previewer should not be invoke during standard execution");
        }

        _tagService = null!; // App.Current.Services.GetRequiredService<ITagToolBackend>().GetTagService();
        var _ = Dispatcher.UIThread.InvokeAsync(UpdateTags, DispatcherPriority.Background);

        DisplayName = "TestDisplayName";
        AssociatedTags.AddRange(new[] { new Tag("Tag1"), new Tag("Tag2"), new Tag("Tag3") });
    }

    public TaggableItemViewModel(TagService.TagServiceClient tagServiceClient)
    {
        _tagService = tagServiceClient;
        // todo: add cancellation token support (tags should not be loaded for example when folder has been exited or tags are not visible)
        var _ = Dispatcher.UIThread.InvokeAsync(UpdateTags, DispatcherPriority.Background);
    }

    [RelayCommand]
    private async Task TagIt(string tagName)
    {
        var tag = Any.Pack(new NormalTag { Name = tagName });
        var tagRequest = TaggedItemType switch
        {
            TaggedItemType.File => new TagItemRequest { Tag = tag, File = new FileDto { Path = Location } },
            TaggedItemType.Folder => new TagItemRequest { Tag = tag, Folder = new FolderDto { Path = Location } },
            _ => throw new UnreachableException()
        };

        var tagReply = await _tagService.TagItemAsync(tagRequest);

        switch (tagReply.ResultCase)
        {
            case TagItemReply.ResultOneofCase.TaggedItem:
                AssociatedTags.Add(new Tag(tagName));
                break;
            case TagItemReply.ResultOneofCase.ErrorMessage:
                Debug.WriteLine($"Unable to tag item {tagRequest}");
                break;
            default:
                throw new UnreachableException();
        }
    }

    [RelayCommand]
    private async Task UntagItem(string tagName)
    {
        var tag = Any.Pack(new NormalTag { Name = tagName });
        var untagItemRequest = TaggedItemType switch
        {
            TaggedItemType.File => new UntagItemRequest { Tag = tag, File = new FileDto { Path = Location } },
            TaggedItemType.Folder => new UntagItemRequest { Tag = tag, Folder = new FolderDto { Path = Location } },
            _ => throw new UnreachableException()
        };

        var reply = await _tagService.UntagItemAsync(untagItemRequest);

        switch (reply.ResultCase)
        {
            case UntagItemReply.ResultOneofCase.TaggedItem:
                AssociatedTags.Remove(new Tag(tagName));
                break;
            case UntagItemReply.ResultOneofCase.ErrorMessage:
                Debug.WriteLine($"Unable to tag item {untagItemRequest}");
                break;
            default:
                throw new UnreachableException();
        }
    }

    private async Task UpdateTags()
    {
        var getItemInfoRequest = TaggedItemType switch
        {
            TaggedItemType.File => new GetItemRequest { File = new FileDto { Path = Location } },
            TaggedItemType.Folder => new GetItemRequest { Folder = new FolderDto { Path = Location } },
            _ => throw new UnreachableException()
        };

        var getItemReply = await _tagService.GetItemAsync(getItemInfoRequest);

        switch (getItemReply.ResultCase)
        {
            case GetItemReply.ResultOneofCase.TaggedItem:
                AssociatedTags.Clear();
                AssociatedTags.AddRange(getItemReply.TaggedItem.Tags.Select(s => new Tag(s.Unpack<NormalTag>().Name)));
                break;
            case GetItemReply.ResultOneofCase.ErrorMessage:
                Debug.WriteLine("Update tags failed");
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}
