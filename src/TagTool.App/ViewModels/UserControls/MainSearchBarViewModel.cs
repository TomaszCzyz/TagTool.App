using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DynamicData;
using JetBrains.Annotations;
using TagTool.App.Core.Models;

namespace TagTool.App.ViewModels.UserControls;

public partial class MainSearchBarViewModel : ViewModelBase, ITagsContainer
{
    [ObservableProperty]
    private string? _searchText;

    [ObservableProperty]
    private Tag? _selectedItemFromSearched;

    [ObservableProperty]
    private Tag? _selectedItemFromPopular;

    public ObservableCollection<Tag> SearchResults { get; set; } = new();

    public ObservableCollection<Tag> PopularTags { get; set; } = new();

    public ObservableCollection<object> EnteredTags { get; set; } = new();

    public Tag[] Tags => EnteredTags.Where(o => o.GetType() == typeof(Tag)).Cast<Tag>().ToArray();

    /// <summary>
    ///     ctor for XAML previewer
    /// </summary>
    public MainSearchBarViewModel()
    {
    }

    [UsedImplicitly]
    public MainSearchBarViewModel(IServiceProvider serviceProvider)
    {
        var tags = new Tag[] { new("Audio"), new("Dog"), new("Picture"), new("Colleague"), new("Tag6"), new("LastTag") };
        EnteredTags.AddRange(tags);

        var popularTags = new Tag[] { new("SomeTag"), new("Tag"), new("SomeTag"), new("Picture"), new("Tag"), new("Picture"), new("Picture") };
        PopularTags.AddRange(popularTags);

        var searchResults = new Tag[] { new("Result1"), new("Tag"), new("Result2"), new("Picture"), new("SearchTag") };
        SearchResults.AddRange(searchResults);

        EnteredTags.Add("");
    }

    [RelayCommand]
    private void RemoveTag(Tag tag)
    {
        Remove(tag);
    }

    [RelayCommand]
    private void UpdateSearch()
    {
        SelectedItemFromSearched = SearchResults.FirstOrDefault();
    }

    [RelayCommand]
    private void AddTag()
    {
        var itemToAdd = SelectedItemFromSearched ?? SelectedItemFromPopular;

        if (itemToAdd is null || EnteredTags.Contains(itemToAdd)) return;

        SearchText = "";
        SearchResults.Remove(itemToAdd);

        AddTag(itemToAdd);
    }

    public void AddTag(Tag tag)
    {
        EnteredTags.Insert(EnteredTags.Count - 1, tag);
    }

    public void Remove(Tag tag)
    {
        EnteredTags.Remove(tag);
    }

    public void RemoveLast()
    {
        var lastTag = EnteredTags.LastOrDefault(o => o.GetType() == typeof(Tag));

        if (lastTag is null) return;

        EnteredTags.Remove(lastTag);
    }
}
