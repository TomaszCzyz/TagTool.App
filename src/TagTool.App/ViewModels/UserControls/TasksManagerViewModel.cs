using System.Collections.ObjectModel;
using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Threading;
using Dock.Model.Mvvm.Controls;
using DynamicData;
using Grpc.Core;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using TagTool.App.Core;
using TagTool.App.Core.Models;
using TagTool.App.Core.Services;
using TagTool.App.Core.TagMapper;
using TagTool.Backend;

namespace TagTool.App.ViewModels.UserControls;

public class TasksManagerViewModel : Document
{
    private readonly IServiceProvider _serviceProvider;
    private readonly TagService.TagServiceClient _tagService;

    public ObservableCollection<TaskViewModel> TaskViewModels { get; } = [];

    public IList<JobInfo> Jobs { get; } = new List<JobInfo>();

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
        _serviceProvider = AppTemplate.Current.Services.GetRequiredService<IServiceProvider>();

        Initialize();
    }

    [UsedImplicitly]
    public TasksManagerViewModel(ITagToolBackend tagToolBackend, IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _tagService = tagToolBackend.GetTagService();

        Initialize();
    }

    private void Initialize()
    {
        Dispatcher.UIThread.InvokeAsync(GetAvailableJobs);
        Dispatcher.UIThread.InvokeAsync(async () =>
        {
            using var serviceScope = _serviceProvider.CreateScope();

            var streamingCall = _tagService.GetExistingTasks(new GetExistingTasksRequest());
            await foreach (var reply in streamingCall.ResponseStream.ReadAllAsync().AsAsyncEnumerable())
            {
                var triggers = reply.Triggers.Select(info =>
                {
                    var taskTriggerViewModel = serviceScope.ServiceProvider.GetRequiredService<TaskTriggerViewModel>();
                    taskTriggerViewModel.TriggerTypeSelectedItem = MapTriggerInfo(info.Type);
                    AssignTaskTriggerValues(info, taskTriggerViewModel);

                    return taskTriggerViewModel;
                });
                var taskViewModel = serviceScope.ServiceProvider.GetRequiredService<TaskViewModel>();

                taskViewModel.TaskId = reply.TaskId;
                taskViewModel.SearchBarViewModel.QuerySegments.AddRange(reply.QueryParams.Select(param => param.MapFromDto()));
                taskViewModel.Triggers.AddRange(triggers);
                taskViewModel.SelectedJob = Jobs.First(info => info.Name == reply.ActionId);

                TaskViewModels.Add(taskViewModel);
            }
        });
    }

    private static void AssignTaskTriggerValues(TriggerInfo info, TaskTriggerViewModel taskTriggerViewModel)
    {
        switch (info.Type)
        {
            case Backend.TriggerType.Event:
                taskTriggerViewModel.TriggerTypeSelectedItem = TriggerType.Event;
                taskTriggerViewModel.EventTypeSelectedItem = info.Arg;
                break;
            case Backend.TriggerType.Cron:
                taskTriggerViewModel.TriggerTypeSelectedItem = TriggerType.Schedule;
                var preDefinedOption = taskTriggerViewModel.PredefinedCronOptionsMap.FirstOrDefault(pair => pair.Value == info.Arg);
                if (preDefinedOption.Equals(default(KeyValuePair<string, string>)))
                {
                    taskTriggerViewModel.CronPredefineOptionSelectedItem = "custom";
                    taskTriggerViewModel.CustomCronText = info.Arg;
                }
                else
                {
                    taskTriggerViewModel.CronPredefineOptionSelectedItem = preDefinedOption.Key;
                }

                break;
        }
    }

    private void GetAvailableJobs()
    {
        var reply = _tagService.GetAvailableActions(new GetAvailableActionsRequest());

        var jobInfos = reply.Infos.Select(info => new JobInfo
        {
            Name = info.Id,
            Description = info.Description,
            ArgumentsExample = string.Join('\n', info.AttributesDescriptions.Values.Select(pair => $"{pair.Key}: {pair.Value}")),
            ApplicableToItemType = info.ApplicableToItemTypes.Select(TagMapper.MapToDomain).OfType<ItemTypeTag>().ToArray()
        });

        Jobs.AddRange(jobInfos);
        OnPropertyChanged(nameof(Jobs));
    }

    public static TriggerType MapTriggerInfo(TagTool.Backend.TriggerType type)
        => type switch
        {
            Backend.TriggerType.Cron => TriggerType.Schedule,
            Backend.TriggerType.Event => TriggerType.Event,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
}
