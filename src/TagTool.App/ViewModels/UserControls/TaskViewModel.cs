using System.Collections.ObjectModel;
using System.Diagnostics;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using TagTool.App.Core;
using TagTool.App.Core.Services;
using TagTool.App.Core.ViewModels;

namespace TagTool.App.ViewModels.UserControls;

public enum TriggerType
{
    Schedule = 0,
    Event = 1
}

public class JobInfo
{
    public required string Name { get; init; }
    public required string Description { get; init; }
    public required string ArgumentsExample { get; init; }
}

public partial class TaskViewModel : ViewModelBase
{
    [ObservableProperty]
    private bool _isEditing;

    [ObservableProperty]
    private string _jobArguments;

    public TaggableItemsSearchBarViewModel SearchBarViewModel { get; }

    public ObservableCollection<object> Triggers { get; } = new();

    public ICollection<JobInfo> Jobs { get; } = new[]
    {
        new JobInfo
        {
            Name = "MoveFileJob",
            Description = "Moves file to a specified location.",
            ArgumentsExample =
                """
                "to": "<destination folder full path>"
                "create_subfolder_from_extension": bool
                """
        },
        new JobInfo
        {
            Name = "CopyFileJob",
            Description = "Copies file to a specified location.",
            ArgumentsExample =
                """
                "to": "<destination folder full path>"
                "create_subfolder_from_extension": bool
                """
        }
    };

    /// <summary>
    ///     ctor for XAML previewer
    /// </summary>
    public TaskViewModel()
    {
        if (!Design.IsDesignMode)
        {
            Debug.Fail("ctor for XAML Previewer should not be invoke during standard execution");
        }

        // _tagService = AppTemplate.Current.Services.GetRequiredService<ITagToolBackend>().GetTagService();
        SearchBarViewModel = AppTemplate.Current.Services.GetRequiredService<TaggableItemsSearchBarViewModel>();
    }

    [UsedImplicitly]
    public TaskViewModel(TaggableItemsSearchBarViewModel taggableItemsSearchBarViewModel, ITagToolBackend tagToolBackend)
    {
        // _tagService = tagToolBackend.GetTagService();
        SearchBarViewModel = taggableItemsSearchBarViewModel;
    }

    [RelayCommand]
    private Task UpdateTask()
    {
        IsEditing ^= true;

        return Task.CompletedTask;
    }
}
