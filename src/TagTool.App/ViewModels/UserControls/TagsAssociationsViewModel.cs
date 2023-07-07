using System.Collections.ObjectModel;
using System.Diagnostics;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using TagTool.App.Core.Models;
using TagTool.App.Core.Services;
using TagTool.App.Core.ViewModels;
using TagTool.Backend;

namespace TagTool.App.ViewModels.UserControls;

public record AssociationData(List<ITag> Synonyms, List<ITag> HigherTags);

public partial class TagsAssociationsViewModel : ViewModelBase
{
    private readonly TagService.TagServiceClient _tagService;

    [ObservableProperty]
    private ObservableCollection<AssociationData> _associationData
        = new()
        {
            new AssociationData(new List<ITag> { new TextTag { Name = "Cat" }, new TextTag { Name = "Pussy" } },
                new List<ITag> { new TextTag { Name = "Animal" }, new TextTag { Name = "Creature" } }),
            new AssociationData(new List<ITag> { new TextTag { Name = "Dog" } },
                new List<ITag> { new TextTag { Name = "Animal" }, new TextTag { Name = "Creature" } })
        };

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
    }
    
    [UsedImplicitly]
    public TagsAssociationsViewModel(ITagToolBackend toolBackend)
    {
        _tagService = toolBackend.GetTagService();
    }

    private void Initialize()
    {
        
    }
    
}
