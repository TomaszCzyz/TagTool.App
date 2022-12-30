using System.Collections.ObjectModel;
using Avalonia.Controls.Documents;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using DynamicData;
using TagTool.App.Core.Models;

namespace TagTool.App.ViewModels.UserControls;

public interface IFileSystemEntry
{
}

public partial class FolderEntryViewModel : ViewModelBase, IFileSystemEntry
{
    [ObservableProperty]
    private string _fullName = "";

    [ObservableProperty]
    private InlineCollection _inlines = new();

    [ObservableProperty]
    private bool _areTagsVisible = true;

    private ObservableCollection<Tag> AssociatedTags { get; } = new();

    public FolderEntryViewModel()
    {
        FullName = "defaultFileName.txt";
        Inlines.Add(new Run { Text = "123", Background = Brushes.Blue });
        AssociatedTags.AddRange(new[] { new Tag("Tag1"), new Tag("Second") });
    }
}
