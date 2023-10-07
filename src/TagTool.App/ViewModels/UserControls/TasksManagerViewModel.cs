using System.Collections.ObjectModel;
using System.Diagnostics;
using Avalonia.Controls;
using Dock.Model.Mvvm.Controls;
using DynamicData;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using TagTool.App.Core;
using TagTool.App.Core.Models;
using TagTool.App.Core.Services;
using TagTool.Backend;

namespace TagTool.App.ViewModels.UserControls;

public class TasksManagerViewModel : Document
{
    private readonly TagService.TagServiceClient _tagService;

    public ObservableCollection<TaskViewModel> TaskViewModels { get; } = new();

    /// <summary>
    ///     ctor for XAML previewer
    /// </summary>
    public TasksManagerViewModel()
    {
        if (!Design.IsDesignMode)
        {
            Debug.Fail("ctor for XAML Previewer should not be invoke during standard execution");
        }

        // _logger = AppTemplate.Current.Services.GetRequiredService<ILogger<TaskViewModel>>();
        _tagService = AppTemplate.Current.Services.GetRequiredService<ITagToolBackend>().GetTagService();

        Initialize();
    }

    [UsedImplicitly]
    public TasksManagerViewModel(ITagToolBackend tagToolBackend)
    {
        _tagService = tagToolBackend.GetTagService();

        Initialize();
    }

    private void Initialize()
    {
        var taskViewModel = AppTemplate.Current.Services.GetRequiredService<TaskViewModel>();
        taskViewModel.TaskId = "Task1";
        taskViewModel.SearchBarViewModel.QuerySegments.AddRange(
            new QuerySegment[]
            {
                new() { Tag = new ItemTypeTag { DisplayText = "File" } }, new() { Tag = new TextTag { Name = "TestTag" } },
                new() { Tag = new TextTag { Name = "TestTag2" }, State = QuerySegmentState.Exclude }
            });
        taskViewModel.SelectedJob = taskViewModel.Jobs.First();
        taskViewModel.Triggers = new ObservableCollection<TaskTriggerViewModel>
        {
            new() { TriggerTypeSelectedItem = TriggerType.Schedule, CronPredefineOptionSelectedItem = "15 minutes" }
        };
        TaskViewModels.Add(taskViewModel);
    }
}
