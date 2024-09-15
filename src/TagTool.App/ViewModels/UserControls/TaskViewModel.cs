using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Avalonia.Controls;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Google.Protobuf.Collections;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TagTool.App.Core;
using TagTool.App.Core.Models;
using TagTool.App.Core.Services;
using TagTool.App.Core.ViewModels;
using TagTool.Backend;

namespace TagTool.App.ViewModels.UserControls;

public class JobInfo
{
    public required string Name { get; init; }
    public required string Description { get; init; }
    public required string ArgumentsExample { get; init; }
    public required ItemTypeTag[] ApplicableToItemType { get; init; }

    private bool Equals(JobInfo other) => Name == other.Name;

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj))
        {
            return false;
        }

        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        return obj.GetType() == GetType() && Equals((JobInfo)obj);
    }

    public override int GetHashCode() => Name.GetHashCode();
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

    [ObservableProperty]
    private string? _selectedJobError;

    [ObservableProperty]
    private string? _jobAttributesError;

    public TaggableItemsSearchBarViewModel SearchBarViewModel { get; }

    public ObservableCollection<TaskTriggerViewModel> Triggers { get; set; } = [];

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
        Dispatcher.UIThread.Post(() => SearchBarViewModel.QuerySegments.CollectionChanged += (_, _) => TryValidateTagQuery(out _));
    }

    partial void OnSelectedJobChanged(JobInfo? value)
    {
        _ = TryValidateJob(out _);
        if (value is null)
        {
            return;
        }

        JobArguments = value.ArgumentsExample;
    }

    partial void OnJobAttributesErrorChanged(string? value) => _ = TryParseAttributes(out _);

    [RelayCommand]
    private void AddTrigger()
    {
        Triggers.Add(new TaskTriggerViewModel());
    }

    [RelayCommand]
    private async Task UpdateTask()
    {
        if (IsEditing)
        {
            return;
        }

        var tryValidateTagQuery = TryValidateTagQuery(out var tagQuery);
        var tryValidateJob = TryValidateJob(out var job);
        var tryParseAttributes = TryParseAttributes(out var attributes);

        if (!tryValidateTagQuery || !tryValidateJob || !tryParseAttributes)
        {
            IsEditing = true;
            return;
        }

        await UpdateTaskInner(job, tagQuery, attributes);

        IsEditing = false;
    }

    private async Task UpdateTaskInner(JobInfo? job, ICollection<QuerySegment>? tagQuery, Attributes attributes)
    {
        Debug.Assert(job != null, nameof(job) + " != null");
        Debug.Assert(tagQuery != null, nameof(tagQuery) + " != null");

        var request = new AddOrUpdateTaskRequest
        {
            TaskId = TaskId,
            ActionId = job.Name,
            ActionAttributes = attributes,
            QueryParams = { tagQuery.Select(segment => segment.MapToDto()) },
            Triggers = { Triggers.Select(MapTriggerInfo) }
        };

        _logger.LogDebug("Sending request {@AddOrUpdateJobRequest}", request);
        var reply = await _tagService.AddOrUpdateTaskAsync(request);
    }

    private static TriggerInfo MapTriggerInfo(TaskTriggerViewModel model)
        => new() { Type = MapTriggerType(model.TriggerTypeSelectedItem), Arg = MapArgs(model) };

    private bool TryParseAttributes(out Attributes attributes)
    {
        attributes = new Attributes();
        foreach (var line in JobArguments.Split('\n'))
        {
            var keyAndValue = line.Split(':');
            if (keyAndValue.Length != 2)
            {
                JobAttributesError = $"Each line should contain key value pair separated with ':',\nincorrect line: {line}";
                return false;
            }

            var key = keyAndValue[0].Trim('\"');
            var value = keyAndValue[1].Trim('\"');
            var mapField = new MapField<string, string> { { key, value } };
            attributes.Values.Add(mapField);
        }

        JobAttributesError = "";
        return true;
    }

    private bool TryValidateTagQuery([NotNullWhen(true)] out ICollection<QuerySegment>? tagQuery)
    {
        tagQuery = null;
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
        tagQuery = SearchBarViewModel.QuerySegments;
        return true;
    }

    private bool TryValidateJob([NotNullWhen(true)] out JobInfo? jobInfo)
    {
        jobInfo = null;
        if (SelectedJob is null)
        {
            SelectedJobError = "Job has to be selected.";
            return false;
        }

        var queriedTypeTags = SearchBarViewModel.QuerySegments.Select(segment => segment.Tag).OfType<ItemTypeTag>().ToArray();

        if (queriedTypeTags.Length == 0)
        {
            SelectedJobError = "At least one one item type tag has to be provided in a tag query.";
            return false;
        }

        if (queriedTypeTags.Any(typeTag => !SelectedJob.ApplicableToItemType.Contains(typeTag)))
        {
            var s = string.Join(", ", SelectedJob.ApplicableToItemType.Select(tag => tag.DisplayText));
            SelectedJobError = $"Selected Job can only be applied to items of type(s): {s}.";
            return false;
        }

        SelectedJobError = "";
        jobInfo = SelectedJob;
        return true;
    }

    private static string MapArgs(TaskTriggerViewModel model)
        => model.TriggerTypeSelectedItem switch
        {
            TriggerType.Schedule => model.Cron!,
            TriggerType.Event => model.EventTypeSelectedItem,
            _ => throw new UnreachableException()
        };

    private static Backend.TriggerType MapTriggerType(TriggerType? triggerType)
        => triggerType switch
        {
            TriggerType.Schedule => Backend.TriggerType.Cron,
            TriggerType.Event => Backend.TriggerType.Event,
            _ => throw new UnreachableException()
        };
}
