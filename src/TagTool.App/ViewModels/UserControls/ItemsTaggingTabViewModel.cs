using System.Collections.ObjectModel;
using System.Diagnostics;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.Input;
using Dock.Model.Mvvm.Controls;
using DynamicData;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using TagTool.App.Services;
using TagTool.Backend;

namespace TagTool.App.ViewModels.UserControls;

public partial class ItemsTaggingTabViewModel : Document
{
    private readonly TagService.TagServiceClient _tagService;
    public ObservableCollection<TaggableItemViewModel> ItemsToTag { get; set; } = new();

    /// <summary>
    ///     ctor for XAML previewer
    /// </summary>
    public ItemsTaggingTabViewModel()
    {
        if (!Design.IsDesignMode)
        {
            Debug.Fail("ctor for XAML Previewer should not be invoke during standard execution");
        }

        _tagService = App.Current.Services.GetRequiredService<ITagToolBackend>().GetTagService();

        var dir = new DirectoryInfo(@"C:\Users\tczyz\MyFiles");
        var files = dir.EnumerateFiles("*", new EnumerationOptions { IgnoreInaccessible = true })
            .Select(info
                => new TaggableItemViewModel(_tagService)
                {
                    TaggedItemType = TaggedItemType.File,
                    DisplayName = info.Name,
                    Location = info.FullName,
                    DateCreated = info.CreationTime,
                    AreTagsVisible = true,
                    Size = info.Length
                })
            .ToArray();

        ItemsToTag.AddRange(files);
    }

    [UsedImplicitly]
    public ItemsTaggingTabViewModel(ITagToolBackend tagToolBackend)
    {
        _tagService = tagToolBackend.GetTagService();
    }

    [RelayCommand]
    public void AddItemsToList(IEnumerable<FileSystemInfo> infos)
    {
        foreach (var info in infos)
        {
            var taggableItemViewModel = info switch
            {
                DirectoryInfo dirInfo => new TaggableItemViewModel(_tagService)
                {
                    TaggedItemType = TaggedItemType.Folder,
                    DisplayName = dirInfo.Name,
                    Location = dirInfo.FullName,
                    DateCreated = dirInfo.CreationTime,
                    AreTagsVisible = true,
                    Size = null
                },
                FileInfo fileInfo => new TaggableItemViewModel(_tagService)
                {
                    TaggedItemType = TaggedItemType.File,
                    DisplayName = fileInfo.Name,
                    Location = fileInfo.FullName,
                    DateCreated = fileInfo.CreationTime,
                    AreTagsVisible = true,
                    Size = fileInfo.Length
                },
                _ => throw new ArgumentOutOfRangeException(nameof(infos))
            };

            ItemsToTag.Add(taggableItemViewModel);
        }
    }
}
