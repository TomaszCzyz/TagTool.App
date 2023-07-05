using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Google.Protobuf.WellKnownTypes;
using TagTool.App.Core.Extensions;
using TagTool.App.Core.Models;
using TagTool.Backend;
using TagTool.Backend.DomainTypes;
using MonthTag = TagTool.App.Core.Models.MonthTag;

namespace TagTool.App.Core.ViewModels;

/// <summary>
///     Basic representation of item that might be tagged, that should be use to display only.
///     This class will react ot updates of underlining <see cref="TaggableItem"/>
///     Contains only information how to display item DisplayName and its tags, if exist.
///     Additional functionalities, like drop-to-tag, should be implemented in parent components.
/// </summary>
[DebuggerDisplay("{DisplayName}")]
public partial class TaggableItemViewModel : ViewModelBase
{
    private readonly TagService.TagServiceClient _tagService;
    private TaggableItem _taggableItem;

    [ObservableProperty]
    private bool _areTagsVisible = true;

    public required TaggableItem TaggableItem
    {
        get => _taggableItem;
        [MemberNotNull(nameof(_taggableItem))]
        set
        {
            if (Equals(value, _taggableItem)) return;
            _taggableItem = value;

            OnPropertyChanged();
            OnPropertyChanged(nameof(DisplayName));
            OnPropertyChanged(nameof(AssociatedTags));
        }
    }

    public string DisplayName => TaggableItem.DisplayName;

    public IReadOnlySet<ITag>? AssociatedTags => TaggableItem.Tags;

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
        TaggableItem = new TaggableFile
        {
            Path = @"C:\Users\tczyz\MyFiles\FromOec\DigitalSign.gif",
            Tags = new HashSet<ITag>(new ITag[] { new TextTag { Name = "Tag1" }, new MonthTag { Month = 3 }, new TextTag { Name = "Tag3" } })
        };
    }

    public TaggableItemViewModel(TagService.TagServiceClient tagServiceClient)
    {
        // todo: add cancellation token support (tags should not be loaded for example when folder has been exited or tags are not visible)
        _tagService = tagServiceClient;

        if (AreTagsVisible)
        {
            Dispatcher.UIThread.InvokeAsync(UpdateTaggableItem, DispatcherPriority.Background);
        }
    }

    [RelayCommand]
    private async Task ExecuteLinkedAction()
    {
        var request = TaggableItem switch
        {
            TaggableFile file => new ExecuteLinkedActionRequest { File = new FileDto { Path = file.Path } },
            TaggableFolder folder => new ExecuteLinkedActionRequest { Folder = new FolderDto { Path = folder.Path } },
            _ => throw new UnreachableException()
        };

        var reply = await _tagService.ExecuteLinkedActionAsync(request);

        if (reply.ResultCase == ExecuteLinkedActionReply.ResultOneofCase.Error)
        {
            WeakReferenceMessenger.Default.Send(new Notification("Error occured", "Unable to execute action", NotificationType.Error));
        }
    }

    [RelayCommand]
    private async Task TagIt(ITag tag)
    {
        var anyTag = Any.Pack(TagMapper.TagMapper.MapToDto(tag));
        var tagRequest = TaggableItem switch
        {
            TaggableFile file => new TagItemRequest { Tag = anyTag, File = new FileDto { Path = file.Path } },
            TaggableFolder folder => new TagItemRequest { Tag = anyTag, Folder = new FolderDto { Path = folder.Path } },
            _ => throw new UnreachableException()
        };

        var tagReply = await _tagService.TagItemAsync(tagRequest);

        switch (tagReply.ResultCase)
        {
            case TagItemReply.ResultOneofCase.TaggedItem:
                TaggableItem = TaggableItem switch
                {
                    TaggableFile file => new TaggableFile { Path = file.Path, Tags = tagReply.TaggedItem.Tags.MapToDomain().ToHashSet() },
                    TaggableFolder folder => new TaggableFolder { Path = folder.Path, Tags = tagReply.TaggedItem.Tags.MapToDomain().ToHashSet() },
                    _ => throw new UnreachableException()
                };
                break;
            case TagItemReply.ResultOneofCase.ErrorMessage:
                Debug.WriteLine($"Unable to tag item {tagRequest}");
                break;
            default:
                throw new UnreachableException();
        }
    }

    [RelayCommand]
    private async Task UntagItem(ITag tag)
    {
        var anyTag = Any.Pack(TagMapper.TagMapper.MapToDto(tag));
        var untagItemRequest = TaggableItem switch
        {
            TaggableFile file => new UntagItemRequest { Tag = anyTag, File = new FileDto { Path = file.Path } },
            TaggableFolder folder => new UntagItemRequest { Tag = anyTag, Folder = new FolderDto { Path = folder.Path } },
            _ => throw new UnreachableException()
        };

        var reply = await _tagService.UntagItemAsync(untagItemRequest);

        switch (reply.ResultCase)
        {
            case UntagItemReply.ResultOneofCase.TaggedItem:
                TaggableItem = TaggableItem switch
                {
                    TaggableFile file => new TaggableFile { Path = file.Path, Tags = reply.TaggedItem.Tags.MapToDomain().ToHashSet() },
                    TaggableFolder folder => new TaggableFolder { Path = folder.Path, Tags = reply.TaggedItem.Tags.MapToDomain().ToHashSet() },
                    _ => throw new UnreachableException()
                };
                break;
            case UntagItemReply.ResultOneofCase.ErrorMessage:
                Debug.WriteLine($"Unable to tag item {untagItemRequest}");
                break;
            default:
                throw new UnreachableException();
        }
    }

    private async Task UpdateTaggableItem()
    {
        var getItemInfoRequest = TaggableItem switch
        {
            TaggableFile file => new GetItemRequest { File = new FileDto { Path = file.Path } },
            TaggableFolder folder => new GetItemRequest { Folder = new FolderDto { Path = folder.Path } },
            _ => throw new UnreachableException()
        };

        var reply = await _tagService.GetItemAsync(getItemInfoRequest);

        switch (reply.ResultCase)
        {
            case GetItemReply.ResultOneofCase.TaggedItem:
                TaggableItem = TaggableItem switch
                {
                    TaggableFile file => new TaggableFile { Path = file.Path, Tags = reply.TaggedItem.Tags.MapToDomain().ToHashSet() },
                    TaggableFolder folder => new TaggableFolder { Path = folder.Path, Tags = reply.TaggedItem.Tags.MapToDomain().ToHashSet() },
                    _ => throw new UnreachableException()
                };
                OnPropertyChanged(nameof(AssociatedTags));
                break;
            case GetItemReply.ResultOneofCase.ErrorMessage:
                Debug.WriteLine("Update tags failed");
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}
