using System.Collections.ObjectModel;
using Avalonia.Controls.Documents;
using CommunityToolkit.Mvvm.ComponentModel;
using TagTool.App.Core.Models;

namespace TagTool.App.ViewModels.UserControls;

public partial class TaggableItemViewModel : ViewModelBase, IFileSystemEntry
{
    private TaggedItemType _taggedItemType;

    [ObservableProperty]
    private string _displayName = "";

    [ObservableProperty]
    private string _location = "";

    [ObservableProperty]
    private DateTime _dateCreated;

    [ObservableProperty]
    private long _size;

    [ObservableProperty]
    private InlineCollection _inlines = new();

    [ObservableProperty]
    private bool _areTagsVisible = true;

    public ObservableCollection<Tag> AssociatedTags { get; } = new();

    public TaggableItemViewModel()
    {
        // DisplayName = "defaultFileName.txt";
        // Inlines.Add(new Run { Text = "123", Background = Brushes.Blue });
        // AssociatedTags.AddRange(new[] { new Tag("Tag1"), new Tag("Second") });
    }
}

public enum TaggedItemType
{
    File = 0,
    Folder = 1
}
