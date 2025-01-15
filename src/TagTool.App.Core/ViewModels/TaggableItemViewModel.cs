using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Avalonia.Controls;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using TagTool.App.Core.Models;
using TagTool.App.Core.Services;
using TagTool.BackendNew;
using Tag = TagTool.App.Core.Models.Tag;

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

    public IReadOnlySet<Tag>? AssociatedTags => TaggableItem.Tags;

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
        TaggableItem = new TaggableFile
        {
            Path = @"C:\Users\tczyz\MyFiles\FromOec\DigitalSign.gif",
            Tags = new HashSet<Tag>([new Tag { Text = "Tag1" }, new Tag { Text = "Tag3" }])
        };
    }

    public TaggableItemViewModel(TagService.TagServiceClient tagServiceClient)
    {
        // todo: add cancellation token support (tags should not be loaded for example when folder has been exited or tags are not visible)
        _tagService = tagServiceClient;

        // todo: rework this to use DI properly
        var tagToolBackend = AppTemplate.Current.Services.GetRequiredService<ITagToolBackend>();

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
    }

    private async Task MoveFileToCommonStorage(TaggableFile file)
    {
    }

    [RelayCommand]
    private async Task ExecuteLinkedAction()
    {
    }

    [RelayCommand]
    private async Task TagIt(Tag tag)
    {
        var tagReply = await _tagService.TagItemAsync(new TagItemRequest { TagId = tag.Id.ToString(), ItemId = TaggableItem.Id.ToString() });

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
                // Debug.WriteLine($"Unable to tag item {taggableItemDto}");
                break;
            default:
                throw new UnreachableException();
        }
    }

    [RelayCommand]
    private async Task UntagItem(Tag tag)
    {
        var reply = await _tagService.UntagItemAsync(new UntagItemRequest { TagId = tag.Id.ToString(), ItemId = TaggableItem.Id.ToString() });

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
                // Debug.WriteLine($"Unable to tag item {taggableItemDto}");
                break;
            default:
                throw new UnreachableException();
        }
    }

    private async Task UpdateTaggableItem()
    {
    }
}
