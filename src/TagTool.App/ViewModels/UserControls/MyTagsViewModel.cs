using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DynamicData;
using JetBrains.Annotations;
using TagTool.App.Core.Services;
using TagTool.Backend;

namespace TagTool.App.ViewModels.UserControls;

public partial class MyTagsViewModel : ViewModelBase
{
    private readonly TagService.TagServiceClient _tagService;
    private readonly TagSearchService.TagSearchServiceClient _searchService;

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
        _tagService = null!;
        _searchService = null!;
    }

    public MyTagsViewModel(ITagToolBackend tagToolBackend)
    {
        _tagService = tagToolBackend.GetTagService();
        _searchService = tagToolBackend.GetSearchService();

        var getAllReply = _searchService.GetAll(new Empty());
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
