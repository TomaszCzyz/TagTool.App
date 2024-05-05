using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.DependencyInjection;
using TagTool.App.Core.Extensions;
using TagTool.App.Core.Models;
using TagTool.App.Core.Services;
using TagTool.Backend;
using TagTool.Backend.DomainTypes;
using MonthTag = TagTool.App.Core.Models.MonthTag;

namespace TagTool.App.Core.ViewModels;

public interface ITextSearchable
{
    ReadOnlySpan<char> SearchText { get; }
}

/// <summary>
///     Basic representation of item that might be tagged, that should be used to display only.
///     This class will react ot updates of underlining <see cref="TaggableItem" />
///     Contains only information how to display item DisplayName and its tags, if existed.
///     Additional functionalities, like drop-to-tag, should be implemented in parent components.
/// </summary>
[DebuggerDisplay("{DisplayName}")]
public partial class TaggableItemViewModel : ViewModelBase, ITextSearchable
{
    private readonly TagService.TagServiceClient _tagService;
    private readonly FileActionsService.FileActionsServiceClient _fileActionsService;
    private readonly FolderActionsService.FolderActionsServiceClient _folderActionsService;
    private TaggableItem _taggableItem;

    [ObservableProperty]
    private bool _areTagsVisible = true;

    public required TaggableItem TaggableItem
    {
        get => _taggableItem;
        [MemberNotNull(nameof(_taggableItem))]
        set
        {
            if (Equals(value, _taggableItem))
            {
                return;
            }

            _taggableItem = value;

            OnPropertyChanged();
            OnPropertyChanged(nameof(DisplayName));
            OnPropertyChanged(nameof(AssociatedTags));
        }
    }

    public string DisplayName => TaggableItem.DisplayName;

    public IReadOnlySet<ITag>? AssociatedTags => TaggableItem.Tags;

    public ReadOnlySpan<char> SearchText => DisplayName.AsSpan();

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
        _fileActionsService = null!;
        _folderActionsService = null!;
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

        // todo: rework this to use DI properly
        var tagToolBackend = AppTemplate.Current.Services.GetRequiredService<ITagToolBackend>();
        _fileActionsService = tagToolBackend.GetFileActionsService();
        _folderActionsService = tagToolBackend.GetFolderActionsService();

        if (AreTagsVisible)
        {
            Dispatcher.UIThread.InvokeAsync(UpdateTaggableItem, DispatcherPriority.Background);
        }
    }

    [RelayCommand]
    private async Task MoveToCommonStorage()
    {
        switch (TaggableItem)
        {
            case TaggableFile file:
                await MoveFileToCommonStorage(file);

                break;
            case TaggableFolder folder:
                await MoveFolderToCommonStorage(folder);

                break;
            default:
                throw new UnreachableException();
        }
    }

    private async Task MoveFolderToCommonStorage(TaggableFolder folder)
    {
        var moveFolderRequest = new MoveFolderRequest { Folder = new FolderDto { Path = folder.Path }, Destination = "CommonStorage" };
        // todo: add cancellation option or timeout
        var moveFolderReply = await _folderActionsService.MoveFolderAsync(moveFolderRequest);

        var notification = moveFolderReply.ResultCase switch
        {
            MoveFolderReply.ResultOneofCase.NewLocation
                => new Notification(
                    "Folder moved successfully",
                    $"Successfully moved the folder to {moveFolderReply.NewLocation}."),
            MoveFolderReply.ResultOneofCase.ErrorMessage
                => new Notification(
                    "Failed to move a folder",
                    $"Unable to move the file\nBackend service message:\n{moveFolderReply.ErrorMessage}."),
            _ => throw new UnreachableException()
        };

        WeakReferenceMessenger.Default.Send(notification.ToMessage());
    }

    private async Task MoveFileToCommonStorage(TaggableFile file)
    {
        var moveFileRequest = new MoveFileRequest { File = new FileDto { Path = file.Path }, Destination = "CommonStorage" };
        // todo: add cancellation option or timeout
        var moveFileReply = await _fileActionsService.MoveFileAsync(moveFileRequest);

        var notification = moveFileReply.ResultCase switch
        {
            MoveFileReply.ResultOneofCase.NewLocation
                => new Notification(
                    "File moved successfully",
                    $"Successfully moved the file to {moveFileReply.NewLocation}."),
            MoveFileReply.ResultOneofCase.ErrorMessage
                => new Notification(
                    "Failed to move a file",
                    $"Unable to move a file\nBackend service message:\n{moveFileReply.ErrorMessage}."),
            _ => throw new UnreachableException()
        };

        WeakReferenceMessenger.Default.Send(notification.ToMessage());
    }

    [RelayCommand]
    private async Task ExecuteLinkedAction()
    {
        var taggableItemDto = TaggableItem switch
        {
            TaggableFile file => new TaggableItemDto { File = new FileDto { Path = file.Path } },
            TaggableFolder folder => new TaggableItemDto { Folder = new FolderDto { Path = folder.Path } },
            _ => throw new UnreachableException()
        };

        var reply = await _tagService.ExecuteLinkedActionAsync(new ExecuteLinkedActionRequest { Item = taggableItemDto });

        if (reply.ResultCase == ExecuteLinkedActionReply.ResultOneofCase.Error)
        {
            WeakReferenceMessenger.Default.Send(new Notification("Error occured", "Unable to execute action", NotificationType.Error));
        }
    }

    [RelayCommand]
    private async Task TagIt(ITag tag)
    {
        var anyTag = Any.Pack(TagMapper.TagMapper.MapToDto(tag));
        var taggableItemDto = TaggableItem switch
        {
            TaggableFile file => new TaggableItemDto { File = new FileDto { Path = file.Path } },
            TaggableFolder folder => new TaggableItemDto { Folder = new FolderDto { Path = folder.Path } },
            _ => throw new UnreachableException()
        };

        var tagReply = await _tagService.TagItemAsync(new TagItemRequest { Tag = anyTag, Item = taggableItemDto });

        switch (tagReply.ResultCase)
        {
            case TagItemReply.ResultOneofCase.Item:
                TaggableItem = TaggableItem switch
                {
                    TaggableFile file => new TaggableFile { Path = file.Path, Tags = tagReply.Item.Tags.MapToDomain().ToHashSet() },
                    TaggableFolder folder => new TaggableFolder { Path = folder.Path, Tags = tagReply.Item.Tags.MapToDomain().ToHashSet() },
                    _ => throw new UnreachableException()
                };
                break;
            case TagItemReply.ResultOneofCase.ErrorMessage:
                Debug.WriteLine($"Unable to tag item {taggableItemDto}");
                break;
            default:
                throw new UnreachableException();
        }
    }

    [RelayCommand]
    private async Task UntagItem(ITag tag)
    {
        var anyTag = Any.Pack(TagMapper.TagMapper.MapToDto(tag));
        var taggableItemDto = TaggableItem switch
        {
            TaggableFile file => new TaggableItemDto { File = new FileDto { Path = file.Path } },
            TaggableFolder folder => new TaggableItemDto { Folder = new FolderDto { Path = folder.Path } },
            _ => throw new UnreachableException()
        };

        var reply = await _tagService.UntagItemAsync(new UntagItemRequest { Tag = anyTag, Item = taggableItemDto });

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
                Debug.WriteLine($"Unable to tag item {taggableItemDto}");
                break;
            default:
                throw new UnreachableException();
        }
    }

    private async Task UpdateTaggableItem()
    {
        var taggableItemDto = TaggableItem switch
        {
            TaggableFile file => new TaggableItemDto { File = new FileDto { Path = file.Path } },
            TaggableFolder folder => new TaggableItemDto { Folder = new FolderDto { Path = folder.Path } },
            _ => throw new UnreachableException()
        };

        var reply = await _tagService.GetItemAsync(new GetItemRequest { TaggableItemDto = taggableItemDto });

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
