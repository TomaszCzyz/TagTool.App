using System.Collections.ObjectModel;
using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ExCSS;
using Google.Protobuf.Collections;
using Google.Protobuf.WellKnownTypes;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TagTool.App.Core;
using TagTool.App.Core.Models;
using TagTool.App.Core.Services;
using TagTool.App.Core.TagMapper;
using TagTool.App.Core.ViewModels;
using TagTool.Backend;

namespace TagTool.App.ViewModels.UserControls;

public class JobInfo
{
    public required string Name { get; init; }
    public required string Description { get; init; }
    public required string ArgumentsExample { get; init; }
}

/// <summary>
///     Task - an action that has associated a tag query, triggers and a job.
///     Job - backend functionality that can be invoked as a part of a task.
/// </summary>
public partial class TaskViewModel : ViewModelBase
{
    private readonly ILogger<TaskViewModel> _logger;
    private readonly TagService.TagServiceClient _tagService;

    [ObservableProperty]
    private bool _isEditing;

    [ObservableProperty]
    private string _taskId = Guid.NewGuid().ToString()[..8];

    [ObservableProperty]
    private JobInfo? _selectedJob;

    [ObservableProperty]
    private string _jobArguments = "";

    [ObservableProperty]
    private string? _tagQueryError;

    public TaggableItemsSearchBarViewModel SearchBarViewModel { get; }

    public ObservableCollection<TaskTriggerViewModel> Triggers { get; } = new();

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

        _logger = AppTemplate.Current.Services.GetRequiredService<ILogger<TaskViewModel>>();
        _tagService = AppTemplate.Current.Services.GetRequiredService<ITagToolBackend>().GetTagService();
        SearchBarViewModel = AppTemplate.Current.Services.GetRequiredService<TaggableItemsSearchBarViewModel>();

        Initialize();
    }

    [UsedImplicitly]
    public TaskViewModel(TaggableItemsSearchBarViewModel taggableItemsSearchBarViewModel, ITagToolBackend tagToolBackend,
        ILogger<TaskViewModel> logger)
    {
        _logger = logger;
        _tagService = tagToolBackend.GetTagService();
        SearchBarViewModel = taggableItemsSearchBarViewModel;

        Initialize();
    }

    private void Initialize()
    {
        Dispatcher.UIThread.Post(() => SearchBarViewModel.QuerySegments.CollectionChanged += (_, _) => ValidateTagQuery());
    }

    partial void OnSelectedJobChanged(JobInfo? value)
    {
        if (value is null)
        {
            return;
        }

        JobArguments = value.ArgumentsExample;
    }

    [RelayCommand]
    private void AddTrigger()
    {
        Triggers.Add(new TaskTriggerViewModel());
    }

    [RelayCommand]
    private Task UpdateTask()
    {
        if (!ValidateTagQuery())
        {
            IsEditing = true;
            return Task.CompletedTask;
        }

        if (SelectedJob is null)
        {
            // todo: it should inform about failure; e.g. Save button could have loading indicator and in a case of a failure
            // button would NOT change state to edit.
            return Task.CompletedTask;
        }
        IsEditing ^= true;

        var attributes = new Attributes();
        foreach (var line in JobArguments.Split(Environment.NewLine))
        {
            var keyAndValue = line.Split(':');
            if (keyAndValue.Length != 2)
            {
                throw new ParseException($"Each line should contain key value pair, incorrect line: {line}");
            }

            var key = keyAndValue[0].Trim('\"');
            var value = keyAndValue[1].Trim('\"');
            var mapField = new MapField<string, string> { { key, value } };
            attributes.Values.Add(mapField);
        }

        var tagQueryParams = SearchBarViewModel.QuerySegments
            .Select(segment =>
                new TagQueryParam { Tag = Any.Pack(TagMapper.MapToDto(segment.Tag)), State = MapQuerySegmentState(segment) });

        var triggers = Triggers.Select(model =>
            new AddOrUpdateJobRequest.Types.TriggerInfo { Type = MapTriggerType(model.TriggerTypeSelectedItem), Arg = MapArgs(model) });

        var request = new AddOrUpdateJobRequest
        {
            TaskId = TaskId,
            JobId = SelectedJob.Name,
            JobAttributes = { attributes },
            QueryParams = { tagQueryParams },
            Triggers = { triggers }
        };

        _logger.LogDebug("Sending request {@AddOrUpdateJobRequest}", request);

        // _tagService.AddOrUpdateJobAsync(request);
        return Task.CompletedTask;
    }

    private bool ValidateTagQuery()
    {
        if (SearchBarViewModel.QuerySegments.Count == 0)
        {
            TagQueryError = "Tag query cannot be empty.";
            return false;
        }

        if (!SearchBarViewModel.QuerySegments.Select(segment => segment.Tag).Any(tag => tag is ItemTypeTag))
        {
            TagQueryError = "Tag query has to contains at least one 'Item Type' tag, e.g. File, Folder";
            return false;
        }

        TagQueryError = "";
        return true;
    }

    private static string MapArgs(TaskTriggerViewModel model)
        => model.TriggerTypeSelectedItem switch
        {
            TriggerType.Schedule => model.Cron!,
            TriggerType.Event => model.EventTypeSelectedItem,
            _ => throw new UnreachableException()
        };

    private static AddOrUpdateJobRequest.Types.TriggerType MapTriggerType(TriggerType? triggerType)
        => triggerType switch
        {
            TriggerType.Schedule => AddOrUpdateJobRequest.Types.TriggerType.Cron,
            TriggerType.Event => AddOrUpdateJobRequest.Types.TriggerType.Event,
            _ => throw new UnreachableException()
        };

    private static TagQueryParam.Types.QuerySegmentState MapQuerySegmentState(QuerySegment segment)
        => segment.State switch
        {
            QuerySegmentState.Exclude => TagQueryParam.Types.QuerySegmentState.Exclude,
            QuerySegmentState.Include => TagQueryParam.Types.QuerySegmentState.Include,
            QuerySegmentState.MustBePresent => TagQueryParam.Types.QuerySegmentState.MustBePresent,
            _ => throw new UnreachableException()
        };
}
