using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using TagTool.App.Core.Models;
using TagTool.App.Core.Services;
using Tag = TagTool.App.Core.Models.Tag;

namespace TagTool.App.Core.Views;

public interface ITextSearchable
{
    ReadOnlySpan<char> SearchText { get; }
}

/// <summary>
///     Basic representation of item that might be tagged, that should be used to display only.
///     This class will react ot updates of underlining <see cref="TaggableItem" />
///     Contains only information how to display item DisplayName and its tags, if existed.
///     Additional functionalities, like drop-to-tag, should be implemented in parent components.
/// </summary>
[DebuggerDisplay("{DisplayName}")]
public partial class TaggableItemViewModel : ViewModelBase, ITextSearchable
{
    private readonly TaggableItemIconResolver _iconResolver;
    private readonly TaggableItemDisplayTextResolver _displayTextResolver;
    private TaggableItem _taggableItem;

    [ObservableProperty]
    private bool _areTagsVisible = true;

    public required TaggableItem TaggableItem
    {
        get => _taggableItem;
        [MemberNotNull(nameof(_taggableItem))]
        set
        {
            if (Equals(value, _taggableItem))
            {
                return;
            }

            _taggableItem = value;

            OnPropertyChanged();
            OnPropertyChanged(nameof(DisplayName));
            OnPropertyChanged(nameof(AssociatedTags));
        }
    }

    public string DisplayName => _displayTextResolver.GetDisplayText(TaggableItem);

    public Bitmap Icon => _iconResolver.GetIcon(TaggableItem, null);

    public ISet<Tag>? AssociatedTags => TaggableItem.Tags;

    public ReadOnlySpan<char> SearchText => DisplayName.AsSpan();

    /// <summary>
    ///     ctor for XAML previewer
    /// </summary>
    public TaggableItemViewModel()
    {
        if (!Design.IsDesignMode)
        {
            Debug.Fail("ctor for XAML Previewer should not be invoke during standard execution");
        }

        _iconResolver = null!;
        _displayTextResolver = null!;
        TaggableItem = new TaggableFile.TaggableFile
        {
            Path = @"C:\Users\tczyz\MyFiles\FromOec\DigitalSign.gif",
            Tags = new HashSet<Tag>([new Tag { Text = "Tag1", Id = 99 }, new Tag { Text = "Tag3", Id = 999 }])
        };
    }

    public TaggableItemViewModel(
        TaggableItemIconResolver iconResolver,
        TaggableItemDisplayTextResolver displayTextResolver)
    {
        _iconResolver = iconResolver;
        _displayTextResolver = displayTextResolver;

        // todo: rework this to use DI properly
        var tagToolBackend = AppTemplate.Current.Services.GetRequiredService<ITagToolBackend>();

        if (AreTagsVisible)
        {
            Dispatcher.UIThread.InvokeAsync(UpdateTaggableItem, DispatcherPriority.Background);
        }
    }

    private async Task UpdateTaggableItem()
    {
    }
}
