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
using Microsoft.Extensions.Logging;
using TagTool.App.Core.Models;
using TagTool.App.Core.Services;
using TagTool.App.Core.TagMapper;
using TagTool.App.Models;
using TagTool.Backend;
using TagTool.Backend.DomainTypes;

namespace TagTool.App.ViewModels.UserControls;

public partial class MyTagsViewModel : Document
{
    private readonly ILogger<MyTagsViewModel> _logger;
    private readonly TagService.TagServiceClient _tagService;

    [ObservableProperty]
    private string? _createTagText;

    [ObservableProperty]
    private string? _selectedTag;

    public ObservableCollection<ITag> AllTags { get; } = new();

    /// <summary>
    ///     ctor for XAML previewer
    /// </summary>
    [UsedImplicitly]
    public MyTagsViewModel()
    {
        if (!Design.IsDesignMode)
        {
            Debug.Fail("ctor for XAML Previewer should not be invoke during standard execution");
        }

        _logger = App.Current.Services.GetRequiredService<ILogger<MyTagsViewModel>>();
        _tagService = App.Current.Services.GetRequiredService<ITagToolBackend>().GetTagService();

        Initialize();
    }

    public MyTagsViewModel(ILogger<MyTagsViewModel> logger, ITagToolBackend tagToolBackend)
    {
        _logger = logger;
        _tagService = tagToolBackend.GetTagService();

        Initialize();
    }

    private void Initialize()
    {
        Dispatcher.UIThread.InvokeAsync(async () =>
        {
            var searchTagsRequest = new SearchTagsRequest { SearchText = "*", SearchType = SearchTagsRequest.Types.SearchType.Wildcard, ResultsLimit = 20 };
            var streamingCall = _tagService.SearchTags(searchTagsRequest);
            await streamingCall.ResponseStream
                .ReadAllAsync()
                .ForEachAsync(reply => AllTags.Add(TagMapper.MapToDomain(reply.Tag)));
        });
    }

    [RelayCommand]
    private void CreateTag()
    {
        if (string.IsNullOrEmpty(CreateTagText)) return;

        var createTagsRequest = new CreateTagRequest { Tag = Any.Pack(new NormalTag { Name = CreateTagText }) };

        _logger.LogInformation("Requesting tag creation {Request}", createTagsRequest);

        var reply = _tagService.CreateTag(createTagsRequest);
        // todo: check is success

        switch (reply.ResultCase)
        {
            case CreateTagReply.ResultOneofCase.ErrorMessage:
                break;
        }

        // todo: make extension method 'AddIfNotExists(..)'
        if (!AllTags.Select(tag => tag.DisplayText).Contains(CreateTagText))
        {
            AllTags.Add(new TextTag { Name = CreateTagText });
        }

        CreateTagText = "";
    }

    [RelayCommand]
    private void RemoveTag(string tagName)
    {
        var deleteTagsRequest = new DeleteTagRequest { Tag = Any.Pack(new NormalTag { Name = tagName }), DeleteUsedToo = false };

        _logger.LogInformation("Requesting tag deletion {Request}", deleteTagsRequest);

        var deleteTagsReply = _tagService.DeleteTag(deleteTagsRequest);

        switch (deleteTagsReply.ResultCase)
        {
            case DeleteTagReply.ResultOneofCase.DeletedTagName:
                var first = AllTags.First(tag => tag.DisplayText == deleteTagsReply.DeletedTagName);
                AllTags.Remove(first);
                break;
            case DeleteTagReply.ResultOneofCase.ErrorMessage:
                var notification = new Notification(
                    "Fail to remove tag",
                    $"Tag {tagName} wan not remove.\nBackend service message:\n{deleteTagsReply.ErrorMessage}",
                    NotificationType.Warning);

                WeakReferenceMessenger.Default.Send(new NewNotificationMessage(notification));
                break;
            case DeleteTagReply.ResultOneofCase.None:
                break;
            default:
                throw new ArgumentOutOfRangeException(deleteTagsReply.ResultCase.ToString());
        }
    }

    [RelayCommand]
    private void NewNot()
    {
        WeakReferenceMessenger.Default.Send(new NewNotificationMessage(new Notification("title", "message from MyTagsViewModel")));
    }
}
