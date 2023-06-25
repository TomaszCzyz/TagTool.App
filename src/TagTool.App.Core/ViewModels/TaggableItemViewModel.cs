using System.Collections.ObjectModel;
using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DynamicData;
using Google.Protobuf.WellKnownTypes;
using TagTool.App.Core.Extensions;
using TagTool.App.Core.Models;
using TagTool.Backend;
using TagTool.Backend.DomainTypes;
using MonthTag = TagTool.App.Core.Models.MonthTag;

namespace TagTool.App.Core.ViewModels;

[DebuggerDisplay("{NewDisplayName}")]
public partial class TaggableItemViewModel : ViewModelBase
{
    private readonly TagService.TagServiceClient _tagService;

    public required TaggableItem TaggableItem { get; init; }

    public string NewDisplayName => TaggableItem.DisplayName;

    public ISet<ITag>? NewAssociatedTags => TaggableItem.Tags;

    [ObservableProperty]
    private InlineCollection _inlines = new();

    [ObservableProperty]
    private bool _areTagsVisible = true;

    public ObservableCollection<ITag> AssociatedTags { get; } = new();

    /// <summary>
    ///     ctor for XAML previewer
    /// </summary>
    public TaggableItemViewModel()
    {
        if (!Design.IsDesignMode)
        {
            Debug.Fail("ctor for XAML Previewer should not be invoke during standard execution");
        }

        _tagService = null!;
        var _ = Dispatcher.UIThread.InvokeAsync(UpdateTags, DispatcherPriority.Background);

        TaggableItem = new TaggableFile { Path = @"C:\Users\tczyz\MyFiles\FromOec\DigitalSign.gif" };
        AssociatedTags.AddRange(new ITag[] { new TextTag { Name = "Tag1" }, new MonthTag { Month = 3 }, new TextTag { Name = "Tag3" } });
    }

    public TaggableItemViewModel(TagService.TagServiceClient tagServiceClient)
    {
        // todo: add cancellation token support (tags should not be loaded for example when folder has been exited or tags are not visible)
        _tagService = tagServiceClient;
        Initialize();
    }

    private void Initialize()
    {
        if (AreTagsVisible)
        {
            Dispatcher.UIThread.InvokeAsync(UpdateTags, DispatcherPriority.Background);
        }
    }

    [RelayCommand]
    private async Task TagIt(string tagName)
    {
        var tag = Any.Pack(new NormalTag { Name = tagName });
        var tagRequest = TaggableItem switch
        {
            TaggableFile file => new TagItemRequest { Tag = tag, File = new FileDto { Path = file.Path } },
            TaggableFolder folder => new TagItemRequest { Tag = tag, Folder = new FolderDto { Path = folder.Path } },
            _ => throw new UnreachableException()
        };

        var tagReply = await _tagService.TagItemAsync(tagRequest);

        switch (tagReply.ResultCase)
        {
            case TagItemReply.ResultOneofCase.TaggedItem:
                AssociatedTags.Add(new TextTag { Name = tagName });
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
        var untagItemRequest = TaggableItem switch
        {
            TaggableFile file => new UntagItemRequest { Tag = tag, File = new FileDto { Path = file.Path } },
            TaggableFolder folder => new UntagItemRequest { Tag = tag, Folder = new FolderDto { Path = folder.Path } },
            _ => throw new UnreachableException()
        };

        var reply = await _tagService.UntagItemAsync(untagItemRequest);

        switch (reply.ResultCase)
        {
            case UntagItemReply.ResultOneofCase.TaggedItem:
                AssociatedTags.Remove(new TextTag { Name = tagName });
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
        var getItemInfoRequest = TaggableItem switch
        {
            TaggableFile file => new GetItemRequest { File = new FileDto { Path = file.Path } },
            TaggableFolder folder => new GetItemRequest { Folder = new FolderDto { Path = folder.Path } },
            _ => throw new UnreachableException()
        };

        var getItemReply = await _tagService.GetItemAsync(getItemInfoRequest);

        switch (getItemReply.ResultCase)
        {
            case GetItemReply.ResultOneofCase.TaggedItem:
                TaggableItem.Tags?.AddRange(getItemReply.TaggedItem.Tags.Select(TagMapper.TagMapper.MapToDomain));
                OnPropertyChanged(nameof(NewAssociatedTags));

                // AssociatedTags.Clear();
                // AssociatedTags.AddRange(getItemReply.TaggedItem.Tags.Select(TagMapper.TagMapper.MapToDomain));
                break;
            case GetItemReply.ResultOneofCase.ErrorMessage:
                Debug.WriteLine("Update tags failed");
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}
