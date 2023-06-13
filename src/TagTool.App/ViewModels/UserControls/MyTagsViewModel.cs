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
using TagTool.App.Models.Messages;
using TagTool.App.Services;
using TagTool.Backend;
using TagTool.Backend.DomainTypes;

namespace TagTool.App.ViewModels.UserControls;

public partial class MyTagsViewModel : Document
{
    private readonly ILogger<MyTagsViewModel> _logger;
    private readonly TagService.TagServiceClient _tagService;

    public double Width { get; set; } = 200;

    [ObservableProperty]
    private string? _createTagText;

    [ObservableProperty]
    private string? _selectedTag;

    public ObservableCollection<string> Items { get; } = new();

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
            var searchTagsRequest = new SearchTagsRequest { Name = "*", SearchType = SearchTagsRequest.Types.SearchType.Wildcard, ResultsLimit = 20 };
            var streamingCall = _tagService.SearchTags(searchTagsRequest);
            await streamingCall.ResponseStream
                .ReadAllAsync()
                .ForEachAsync(reply => Items.Add(TagMapper.TagMapper.MapToDomain(reply.Tag).DisplayText));
        });
    }

    [RelayCommand]
    private void CreateTag()
    {
        if (string.IsNullOrEmpty(CreateTagText)) return;

        var createTagsRequest = new CreateTagRequest { Tag = Any.Pack(new NormalTag { Name = CreateTagText }) };

        _logger.LogInformation("Requesting tag creation {Request}", createTagsRequest);

        var createTagsReply = _tagService.CreateTag(createTagsRequest);
        // todo: check is success

        // todo: make extension method 'AddIfNotExists(..)'
        if (!Items.Contains(CreateTagText))
        {
            Items.Add(CreateTagText);
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
                Items.Remove(tagName);
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
