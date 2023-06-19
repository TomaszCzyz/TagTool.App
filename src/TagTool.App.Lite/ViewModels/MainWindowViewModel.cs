using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using TagTool.App.Core.Services;
using TagTool.App.Core.ViewModels;
using TagTool.Backend;

namespace TagTool.App.Lite.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly TagService.TagServiceClient _tagToolBackend;

    [ObservableProperty]
    private TaggableItemsSearchBarViewModel _searchBarViewModel;

    public ObservableCollection<TaggableItemViewModel> SearchResults { get; } = new();

    /// <summary>
    ///     ctor for XAML previewer
    /// </summary>
    public MainWindowViewModel()
    {
        if (!Design.IsDesignMode)
        {
            Debug.Fail("ctor for XAML Previewer should not be invoke during standard execution");
        }

        _tagToolBackend = App.Current.Services.GetRequiredService<ITagToolBackend>().GetTagService();
        SearchBarViewModel = App.Current.Services.GetRequiredService<TaggableItemsSearchBarViewModel>();

        Initialize();
    }

    [UsedImplicitly]
    public MainWindowViewModel(TaggableItemsSearchBarViewModel taggableItemsSearchBarViewModel, ITagToolBackend tagToolBackend)
    {
        _tagToolBackend = tagToolBackend.GetTagService();
        SearchBarViewModel = taggableItemsSearchBarViewModel;

        Initialize();
    }

    private void Initialize()
    {
        // SearchBarViewModel.QuerySegments.CollectionChanged += (sender, args) => 
        SearchBarViewModel.CommitSearchQueryEvent += (sender, args) =>
        {
            SearchResults.Add(new TaggableItemViewModel(_tagToolBackend)
            {
                TaggedItemType = TaggedItemType.File,
                DisplayName = "test",
                Location = "test",
                DateCreated = DateTime.Now,
                AreTagsVisible = true,
                Size = 123123
            });
        };
        
        SearchResults.Add(new TaggableItemViewModel(_tagToolBackend)
        {
            TaggedItemType = TaggedItemType.File,
            DisplayName = "test2",
            Location = "test",
            DateCreated = DateTime.Now,
            AreTagsVisible = true,
            Size = 123123
        });
    }
    //
    // [RelayCommand]
    // private async Task CommitSearch()
    // {
    //     var tagQueryParams = SearchBarViewModel.QuerySegments.Select(segment
    //         => new GetItemsByTagsV2Request.Types.TagQueryParam
    //         {
    //             Tag = TagMapper.MapToDto(segment.Tag),
    //             Include = segment.State == QuerySegmentState.Include,
    //             MustBePresent = segment.State == QuerySegmentState.MustBePresent
    //         });
    //
    //     // todo: inform other component (which is responsible for displaying found items) that query has changed
    //     // var _ = await _tagService.GetItemsByTagsV2Async(new GetItemsByTagsV2Request { QueryParams = { tagQueryParams } });
    // }
}
