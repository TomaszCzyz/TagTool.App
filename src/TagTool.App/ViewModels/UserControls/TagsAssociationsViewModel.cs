using System.Collections.ObjectModel;
using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Dock.Model.Mvvm.Controls;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using TagTool.App.Core;
using TagTool.App.Core.Extensions;
using TagTool.App.Core.Models;
using TagTool.App.Core.Services;
using TagTool.App.Core.TagMapper;
using TagTool.App.Models;
using TagTool.Backend;

namespace TagTool.App.ViewModels.UserControls;

public record AssociationData(string GroupName, List<ITag> Synonyms, List<string> Ancestors);

public partial class TagsAssociationsViewModel : Document
{
    private readonly TagService.TagServiceClient _tagService;

    [ObservableProperty]
    private ObservableCollection<AssociationData> _associationData = new();

    [ObservableProperty]
    private AssociationData? _selectedItem;

    /// <summary>
    ///     ctor for XAML previewer
    /// </summary>
    public TagsAssociationsViewModel()
    {
        if (!Design.IsDesignMode)
        {
            Debug.Fail("ctor for XAML Previewer should not be invoke during standard execution");
        }

        _tagService = AppTemplate.Current.Services.GetRequiredService<ITagToolBackend>().GetTagService();
        ReloadAllRelations();
    }

    [UsedImplicitly]
    public TagsAssociationsViewModel(ITagToolBackend toolBackend)
    {
        _tagService = toolBackend.GetTagService();
        ReloadAllRelations();
    }

    private void ReloadAllRelations()
    {
        var request = new GetAllTagsAssociationsRequest();
        var streamingCall = _tagService.GetAllTagsAssociations(request);

        Dispatcher.UIThread.InvokeAsync(async () =>
        {
            AssociationData.Clear();
            await foreach (var reply in streamingCall.ResponseStream.ReadAllAsync())
            {
                AssociationData.Add(new AssociationData(reply.GroupName, reply.TagsInGroup.MapToDomain().ToList(), reply.ParentGroupNames.ToList()));
            }
        });
    }

    [RelayCommand]
    private async Task AddTagToSynonymsGroup((ITag Tag, string GroupName) input)
    {
        var (tag, groupName) = (input.Tag, input.GroupName);
        var anyTag = Any.Pack(TagMapper.MapToDto(tag));
        var reply = await _tagService.AddSynonymAsync(new AddSynonymRequest { Tag = anyTag, GroupName = groupName });

        var notification = reply.ResultCase switch
        {
            AddSynonymReply.ResultOneofCase.SuccessMessage
                => new Notification("Tag added to synonyms group", $"Successfully add tag {tag.DisplayText} to group {groupName}"),
            AddSynonymReply.ResultOneofCase.Error
                => new Notification(
                    "Failed to add tag to a group",
                    $"Tag {tag.DisplayText} was not added to group {groupName}.\nBackend service message:\n{reply.Error.Message}",
                    NotificationType.Warning),
            _ => throw new ArgumentOutOfRangeException(reply.ResultCase.ToString())
        };

        ReloadAllRelations();

        WeakReferenceMessenger.Default.Send(new NewNotificationMessage(notification));
    }

    [RelayCommand]
    private async Task RemoveTagFromSynonyms(ITag? tag)
    {
        if (SelectedItem is null || tag is null)
        {
            return;
        }

        var groupName = SelectedItem.GroupName;
        var anyTag = Any.Pack(TagMapper.MapToDto(tag));
        var reply = await _tagService.RemoveSynonymAsync(new RemoveSynonymRequest { Tag = anyTag, GroupName = groupName });

        var notification = reply.ResultCase switch
        {
            RemoveSynonymReply.ResultOneofCase.SuccessMessage
                => new Notification("Tag removed from synonyms group", $"Successfully removed tag {tag.DisplayText} from group {groupName}"),
            RemoveSynonymReply.ResultOneofCase.Error
                => new Notification(
                    "Failed to remove tag from the group",
                    $"Tag {tag.DisplayText} was not removed from group {groupName}.\nBackend service message:\n{reply.Error.Message}",
                    NotificationType.Warning),
            _ => throw new ArgumentOutOfRangeException(reply.ResultCase.ToString())
        };

        ReloadAllRelations();

        WeakReferenceMessenger.Default.Send(new NewNotificationMessage(notification));
    }
}
