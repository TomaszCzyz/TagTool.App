using System.Collections.ObjectModel;
using System.Diagnostics;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.Input;
using Dock.Model.Mvvm.Controls;
using DynamicData;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using TagTool.App.Core.Models;
using TagTool.App.Core.Services;
using TagTool.App.Core.ViewModels;
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
        var files = dir
            .EnumerateFiles("*", new EnumerationOptions { IgnoreInaccessible = true })
            .Select(info
                => new TaggableItemViewModel(_tagService) { TaggableItem = new TaggableFile { Path = info.FullName }, AreTagsVisible = true })
            .ToArray();

        ItemsToTag.AddRange(files);
    }

    [UsedImplicitly]
    public ItemsTaggingTabViewModel(ITagToolBackend tagToolBackend)
    {
        _tagService = tagToolBackend.GetTagService();
    }

    [RelayCommand]
    private void AddItemsToList(IEnumerable<FileSystemInfo> infos)
    {
        foreach (var info in infos)
        {
            var taggableItemViewModel = info switch
            {
                DirectoryInfo
                    => new TaggableItemViewModel(_tagService) { TaggableItem = new TaggableFolder { Path = info.FullName }, AreTagsVisible = true },
                FileInfo
                    => new TaggableItemViewModel(_tagService) { TaggableItem = new TaggableFile { Path = info.FullName }, AreTagsVisible = true },
                _ => throw new ArgumentOutOfRangeException(nameof(infos))
            };

            ItemsToTag.Add(taggableItemViewModel);
        }
    }
}
