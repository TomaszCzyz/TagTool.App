using System.Collections.ObjectModel;
using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using Dock.Model.Mvvm.Controls;
using Grpc.Core;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using TagTool.App.Core.Extensions;
using TagTool.App.Core.Models;
using TagTool.App.Core.Services;
using TagTool.Backend;

namespace TagTool.App.ViewModels.UserControls;

public record AssociationData(string GroupName, List<ITag> Synonyms, List<string> Ancestors);

public partial class TagsAssociationsViewModel : Document
{
    private readonly TagService.TagServiceClient _tagService;

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
        Initialize();
    }

    [UsedImplicitly]
    public TagsAssociationsViewModel(ITagToolBackend toolBackend)
    {
        _tagService = toolBackend.GetTagService();
        Initialize();
    }

    private void Initialize()
    {
        // Tag = Any.Pack(TagMapper.MapToDto(new TextTag { Name = "Cat" }))
        var request = new GetAllTagsAssociationsRequest();
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
