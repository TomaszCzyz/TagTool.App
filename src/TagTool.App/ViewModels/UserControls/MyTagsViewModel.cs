using System.Collections.ObjectModel;
using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Dock.Model.Mvvm.Controls;
using DynamicData;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TagTool.App.Core.Services;
using TagTool.App.Models.Messages;
using TagTool.Backend;

namespace TagTool.App.ViewModels.UserControls;

public partial class MyTagsViewModel : Document
{
    private readonly ILogger<MyTagsViewModel> _logger;
    private readonly TagService.TagServiceClient _tagService;
    private readonly TagSearchService.TagSearchServiceClient _tagSearchService;

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
        _tagSearchService = App.Current.Services.GetRequiredService<ITagToolBackend>().GetSearchService();

        Initialize();
    }

    public MyTagsViewModel(ILogger<MyTagsViewModel> logger, ITagToolBackend tagToolBackend)
    {
        _logger = logger;
        _tagService = tagToolBackend.GetTagService();
        _tagSearchService = tagToolBackend.GetSearchService();

        Initialize();
    }

    private void Initialize()
    {
        var getAllReply = _tagSearchService.GetAll(new Empty());
        Items.AddRange(getAllReply.TagNames);
    }

    [RelayCommand]
    private void CreateTag()
    {
        if (string.IsNullOrEmpty(CreateTagText)) return;

        var createTagsRequest = new CreateTagsRequest { TagNames = { CreateTagText } };

        _logger.LogInformation("Requesting tag creation {Request}", createTagsRequest);

        var createTagsReply = _tagService.CreateTags(createTagsRequest);

        // todo: make extension method 'AddIfNotExists(..)'
        if (!Items.Contains(CreateTagText))
        {
            Items.Add(CreateTagText);
        }

        CreateTagText = "";
    }

    [RelayCommand]
    private void NewNot()
    {
        WeakReferenceMessenger.Default.Send(new NewNotificationMessage(new Notification("title", "message from MyTagsViewModel")));
    }
}
