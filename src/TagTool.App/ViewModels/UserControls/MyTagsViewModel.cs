using System.Collections.ObjectModel;
using System.Diagnostics;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Dock.Model.Mvvm.Controls;
using DynamicData;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using TagTool.App.Core.Services;
using TagTool.Backend;

namespace TagTool.App.ViewModels.UserControls;

public partial class MyTagsViewModel : Document
{
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

        _tagService = App.Current.Services.GetRequiredService<ITagToolBackend>().GetTagService();
        _tagSearchService = App.Current.Services.GetRequiredService<ITagToolBackend>().GetSearchService();

        Initialize();
    }

    public MyTagsViewModel(ITagToolBackend tagToolBackend)
    {
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

        var createTagsReply = _tagService.CreateTags(new CreateTagsRequest { TagNames = { CreateTagText } });

        // todo: make extension method 'AddIfNotExists(..)'
        if (!Items.Contains(CreateTagText))
        {
            Items.Add(CreateTagText);
        }

        CreateTagText = "";
    }
}
