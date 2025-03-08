using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text.Json;
using Avalonia.Controls;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DynamicData;
using Grpc.Core;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using TagTool.App.Contracts;
using TagTool.App.Extensions;
using TagTool.App.Models;
using TagTool.App.Services;
using TagTool.BackendNew.Services.Grpc;
using TagTool.BackendNew.Services.Grpc.Dtos;
using Tag = TagTool.App.Contracts.Tag;

namespace TagTool.App.Views;

public enum TriggerType
{
    Event = 0,
    Cron = 1,
}

public static class TriggerTypeExtensions
{
    public static TriggerType MapFromDto(this string triggerType)
        => triggerType switch
        {
            "Event" => TriggerType.Event,
            "Cron" => TriggerType.Cron,
            _ => throw new ArgumentOutOfRangeException(nameof(triggerType), triggerType, null)
        };
}

public interface IPayloadProperty;

// This needs to be viewmodel
public record StringProperty(string Name, bool IsRequired) : IPayloadProperty;

public record TagProperty(string Name, bool IsRequired) : IPayloadProperty;

public record InvocableDefinition
{
    public required string Id { get; init; }
    public required string GroupId { get; init; }
    public required string DisplayName { get; init; }
    public required string Description { get; init; }
    public required TriggerType TriggerType { get; init; }
    public required string Payload { get; init; }
}

public record JobItem(string Id, string Name, string Description);

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly TaggableItemIconResolver _iconResolver;
    private readonly TaggableItemDisplayTextResolver _displayTextResolver;
    private readonly TaggableItemMapper _taggableItemMapper;
    private readonly TagsService.TagsServiceClient _tagService;
    private readonly InvocablesService.InvocablesServiceClient _invocablesServiceClient;

    public TaggableItemsSearchBarViewModel SearchBarViewModel { get; }

    public ObservableCollection<TaggableItemViewModel> SearchResults { get; } = [];

    public Dictionary<Type, string[]> TaggableItemContextMenuActions { get; set; } = [];

    public ObservableCollection<JobItem> JobItems { get; } = [];

    private readonly List<InvocableDefinition> _invocableDefinitions = [];

    [ObservableProperty]
    private JobItem? _selectedJobItem;

    public ObservableCollection<TriggerType> TriggerTypes { get; } = [];

    public bool IsCronVisible => SelectedTriggerType == TriggerType.Cron;

    [ObservableProperty]
    private string _cronExpression = "* * * * *";

    public ObservableCollection<IPayloadProperty> PayloadProperties { get; } = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsCronVisible))]
    private TriggerType? _selectedTriggerType;

    [ObservableProperty]
    private Func<string?, CancellationToken, Task<IEnumerable<object>>> _getTagsAsyncPopulator;

    /// <summary>
    ///     ctor for XAML previewer
    /// </summary>
    public MainWindowViewModel()
    {
        if (!Design.IsDesignMode)
        {
            Debug.Fail("ctor for XAML Previewer should not be invoke during standard execution");
        }

        _displayTextResolver = AppTemplate.Current.Services.GetRequiredService<TaggableItemDisplayTextResolver>();
        _iconResolver = AppTemplate.Current.Services.GetRequiredService<TaggableItemIconResolver>();
        _taggableItemMapper = AppTemplate.Current.Services.GetRequiredService<TaggableItemMapper>();
        _tagService = AppTemplate.Current.Services.GetRequiredService<ITagToolBackend>().GetTagService();
        _invocablesServiceClient = AppTemplate.Current.Services.GetRequiredService<ITagToolBackend>().GetInvocablesService();

        SearchBarViewModel = AppTemplate.Current.Services.GetRequiredService<TaggableItemsSearchBarViewModel>();
        GetTagsAsyncPopulator = GetTagsAsync;

        Initialize();
    }

    [UsedImplicitly]
    public MainWindowViewModel(
        TaggableItemsSearchBarViewModel taggableItemsSearchBarViewModel,
        ITagToolBackend tagToolBackend,
        TaggableItemDisplayTextResolver displayTextResolver,
        TaggableItemIconResolver iconResolver,
        TaggableItemMapper taggableItemMapper)
    {
        _tagService = tagToolBackend.GetTagService();
        _invocablesServiceClient = tagToolBackend.GetInvocablesService();
        _displayTextResolver = displayTextResolver;
        _iconResolver = iconResolver;
        _taggableItemMapper = taggableItemMapper;
        SearchBarViewModel = taggableItemsSearchBarViewModel;
        GetTagsAsyncPopulator = GetTagsAsync;

        Initialize();
    }

    private void Initialize()
    {
        SearchBarViewModel.CommitSearchQueryEvent += (_, args) => Dispatcher.UIThread.InvokeAsync(() => SearchForTaggableItems(args.QuerySegments));

        // Initial, empty search.
        Dispatcher.UIThread.InvokeAsync(() => SearchForTaggableItems(null));
        Dispatcher.UIThread.InvokeAsync(GetInvocableDescriptions);
        // Fetch TaggableItem-specific operations
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            var reply = _tagService.GetOperations(new GetOperationsRequest());
            TaggableItemContextMenuActions = reply.Operations.ToDictionary(
                o => o.TypeName == "TaggableFile_A8ABBA71" ? typeof(TaggableFile.TaggableFile) : throw new ArgumentOutOfRangeException(),
                o => o.Name.ToArray());
        });
    }

    partial void OnSelectedJobItemChanged(JobItem? value)
    {
        TriggerTypes.Clear();
        PayloadProperties.Clear();

        if (value is null)
        {
            return;
        }

        var invocableDefinition = _invocableDefinitions.First(d => d.Id == value.Id);
        var triggerTypes = _invocableDefinitions
            .Where(d => d.GroupId == invocableDefinition.GroupId)
            .Select(d => d.TriggerType);

        TriggerTypes.AddRange(triggerTypes);
        SelectedTriggerType = TriggerTypes.First();

        using var jsonDocument = JsonDocument.Parse(invocableDefinition.Payload);
        var root = jsonDocument.RootElement;
        var payload = root.GetProperty("properties").GetProperty("Payload");
        var payloadProperties = payload.GetProperty("properties");
        var payloadRequired = payload.GetProperty("required");

        foreach (var property in payloadProperties.EnumerateObject())
        {
            // 'type' can be an array!!!
            var propertyType = property.Value.GetProperty("type");
            if (propertyType.GetString() == "string")
            {
                var isRequired = payloadRequired.EnumerateArray().Any(r => r.GetString() == property.Name);
                PayloadProperties.Add(new StringProperty(property.Name, isRequired));
            }
            else if (propertyType.GetString() == "tag")
            {
                var isRequired = payloadRequired.EnumerateArray().Any(r => r.GetString() == property.Name);
                PayloadProperties.Add(new TagProperty(property.Name, isRequired));
            }
        }
        // PayloadProperties.AddRange(invocableDefinition.Payload)
    }

    public async Task<IEnumerable<object>> GetTagsAsync(string? searchText, CancellationToken cancellationToken)
    {
        var callOptions = new CallOptions()
            .WithCancellationToken(cancellationToken)
            .WithDeadline(DateTime.UtcNow.AddMinutes(1));

        var streamingCall = _tagService.SearchTags(
            new SearchTagsRequest
            {
                SearchText = searchText,
                SearchType = SearchTagsRequest.Types.SearchType.Fuzzy,
                ResultsLimit = 30
            },
            callOptions);

        return await streamingCall.ResponseStream
            .ReadAllAsync(cancellationToken)
            .Select(r => r.Tag.MapFromDto().Text)
            .ToListAsync(cancellationToken);
    }

    [RelayCommand]
    private void ShowNewItemsPanel()
    {
    }

    [RelayCommand]
    private async Task TagItem((TaggableItemBase Item, Tag Tag) args)
    {
        var (item, tag) = args;
        var reply = await _tagService.TagItemAsync(new TagItemRequest { TagId = tag.Id, ItemId = item.Id.ToString() });

        switch (reply.ResultCase)
        {
            case TagItemReply.ResultOneofCase.TaggableItem:
                item.Tags = reply.TaggableItem.Tags.MapFromDto().ToHashSet();
                break;
            case TagItemReply.ResultOneofCase.ErrorMessage:
                Debug.WriteLine("Unable to tag item");
                break;
            default:
                throw new UnreachableException();
        }
    }

    [RelayCommand]
    private async Task UntagItem((TaggableItemBase Item, Tag Tag) args)
    {
        var (item, tag) = args;
        var reply = await _tagService.UntagItemAsync(new UntagItemRequest { TagId = tag.Id, ItemId = item.Id.ToString() });

        // switch (reply.ResultCase)
        // {
        //     case UntagItemReply.ResultOneofCase.TaggedItem:
        //         item.Tags = reply.TaggedItem.Tags.MapFromDto().ToHashSet();
        //         break;
        //     case UntagItemReply.ResultOneofCase.ErrorMessage:
        //         Debug.WriteLine("Unable to untag item");
        //         break;
        //     case UntagItemReply.ResultOneofCase.None:
        //     default:
        //         throw new UnreachableException();
        // }
    }

    private async Task GetInvocableDescriptions()
    {
        var reply = await _invocablesServiceClient.GetInvocablesDescriptionsAsync(new GetInvocablesDescriptionsRequest());
        _invocableDefinitions.Clear();
        _invocableDefinitions.AddRange(reply.InvocableDefinitions
            .Select(d => new InvocableDefinition
            {
                Id = d.Id,
                GroupId = d.GroupId,
                DisplayName = d.DisplayName,
                Description = d.Description,
                TriggerType = d.TriggerType.MapFromDto(),
                Payload = d.PayloadSchema
            }));

        JobItems.Clear();
        JobItems.AddRange(reply.InvocableDefinitions.Select(d => new JobItem(d.Id, d.DisplayName, d.Description)));
        SelectedJobItem = JobItems.FirstOrDefault();
    }

    private async Task SearchForTaggableItems(ICollection<QuerySegment>? argsQuerySegments)
    {
        var tagQueryParams = argsQuerySegments?.Select(segment => segment.MapToDto());

        var reply = await _tagService.GetItemsByTagsAsync(new GetItemsByTagsRequest { QueryParams = { tagQueryParams ?? [] } });

        var taggableItems = reply.TaggedItems
            .Select(i => _taggableItemMapper.MapToObj(i.Item.Type, i.Item.Payload, i.Tags))
            .ToArray();

        SearchResults.Clear();
        SearchResults.AddRange(taggableItems.Select(
            item =>
            {
                var text = _displayTextResolver.GetDisplayText(item);
                var icon = _iconResolver.GetIcon(item, null);
                return new TaggableItemViewModel(item, icon, text);
            }));
    }
}
