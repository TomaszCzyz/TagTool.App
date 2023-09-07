using System.Collections.ObjectModel;
using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DynamicData;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using TagTool.App.Core.Extensions;
using TagTool.App.Core.Models;
using TagTool.App.Core.Services;
using TagTool.App.Core.TagMapper;
using TagTool.App.Core.ViewModels;
using TagTool.Backend;

namespace TagTool.App.ViewModels.UserControls;

public record AssociationData(string GroupName, List<ITag> Synonyms, List<string> Ancestors);

public partial class TagsAssociationsViewModel : ViewModelBase
{
    private readonly TagService.TagServiceClient _tagService;

    [ObservableProperty]
    private ObservableCollection<string> _groupNames = new();

    [ObservableProperty]
    private ObservableCollection<ITag[]> _tagsInGroup = new();

    [ObservableProperty]
    private ObservableCollection<string[]> _ancestors = new();

    [ObservableProperty]
    private ObservableCollection<AssociationData> _associationData = new();

    /// <summary>
    ///     ctor for XAML previewer
    /// </summary>
    public TagsAssociationsViewModel()
    {
        if (!Design.IsDesignMode)
        {
            Debug.Fail("ctor for XAML Previewer should not be invoke during standard execution");
        }

        _tagService = App.Current.Services.GetRequiredService<ITagToolBackend>().GetTagService();
        var animalTag = new TextTag { Name = "Animal" };
        var animalBaseTag = new TextTag { Name = "AnimalBase" };
        var catTag = new TextTag { Name = "Cat" };
        var cat2Tag = new TextTag { Name = "Cat2" };
        var pussyTag = new TextTag { Name = "Pussy" };
        var dogTag = new TextTag { Name = "Dog" };

        GroupNames.Add("Cat Group");
        TagsInGroup.Add(new ITag[] { catTag, cat2Tag, pussyTag });
        Ancestors.Add(new[] { animalTag.DisplayText, animalBaseTag.DisplayText });

        AssociationData.Add(new AssociationData("Animal_TempGroup", new List<ITag> { animalTag }, new List<string> { animalBaseTag.DisplayText }));
        AssociationData.Add(new AssociationData("AnimalBase_TempGroup", new List<ITag> { animalBaseTag }, new List<string>()));
        AssociationData.Add(new AssociationData("Cat Group", new List<ITag> { catTag, cat2Tag, pussyTag },
            new List<string> { animalTag.DisplayText, animalBaseTag.DisplayText }));
        AssociationData.Add(new AssociationData("Dog Group", new List<ITag> { dogTag },
            new List<string> { animalTag.DisplayText, animalBaseTag.DisplayText }));

        AssociationData = new ObservableCollection<AssociationData>(AssociationData.OrderBy(data => data.Ancestors.Count).ToArray());
    }

    [UsedImplicitly]
    public TagsAssociationsViewModel(ITagToolBackend toolBackend)
    {
        _tagService = toolBackend.GetTagService();

        Initialize();
    }

    private void Initialize()
    {
        var request = new GetAllTagsAssociationsRequest { Tag = Any.Pack(TagMapper.MapToDto(new TextTag { Name = "Cat" })) };
        var streamingCall = _tagService.GetAllTagsAssociations(request);

        Dispatcher.UIThread.InvokeAsync(async () =>
        {
            await foreach (var reply in streamingCall.ResponseStream.ReadAllAsync())
            {
                AssociationData.Add(new AssociationData(reply.GroupName, reply.TagsInGroup.MapToDomain().ToList(), reply.ParentGroupNames.ToList()));
            }
        });
    }

    // [RelayCommand]
    // private Task RemoveTagFromSynonyms(ITag tag)
    // {
    // }
}
