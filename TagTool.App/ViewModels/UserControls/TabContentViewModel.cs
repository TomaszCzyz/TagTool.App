using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using Dock.Model.Mvvm.Controls;
using DynamicData;
using File = TagTool.App.Models.File;

namespace TagTool.App.ViewModels.UserControls;

public class Tag
{
    public Tag(string? name)
    {
        Name = name;
    }

    public string? Name { get; set; }
}

public partial class TabContentViewModel : Document
{
    public ObservableCollection<File> Files { get; set; } = new();

    public ObservableCollection<object> EnteredTags { get; set; } = new();

    public TabContentViewModel()
    {
        Files.AddRange(_exampleFiles);
        EnteredTags.AddRange(
            new Tag[] { new("Tag1"), new("Audio"), new("Dog"), new("Picture"), new("Colleague"), new("Tag6"), new("Tag7"), new("LastTag") });
        EnteredTags.Add(new TagSearchBoxViewModel());
    }

    // [RelayCommand]
    // public void AddSearchTag(KeyEventArgs e)
    // {
    //     switch (e.Key)
    //     {
    //         case Key.Enter:
    //             if (string.IsNullOrWhiteSpace(NewSearchTag)) return;
    //
    //             EnteredTags.Insert(EnteredTags.Count - 1, new Tag(NewSearchTag));
    //             NewSearchTag = string.Empty;
    //             return;
    //         case Key.Back: // TODO: Key.Back event is consumed by TextBox, so this case is unreachable
    //             if (!string.IsNullOrWhiteSpace(NewSearchTag)) return;
    //
    //             EnteredTags.RemoveAt(EnteredTags.Count - 2);
    //             return;
    //     }
    // }

    [RelayCommand]
    public void RemoveLastTag()
    {
        if (EnteredTags.Count <= 1) return;

        EnteredTags.RemoveAt(EnteredTags.Count - 2);
    }

    private readonly File[] _exampleFiles =
    {
        new(1, "File1", 1234, new DateTime(2022, 12, 12), null, @"C:\Program Files"),
        new(1, "File1", 1234, new DateTime(2022, 12, 12), null, @"C:\Program Files"),
        new(1, "File1", 1234, new DateTime(2022, 12, 12), null, @"C:\Program Files"),
        new(1, "File1", 1234, new DateTime(2022, 12, 12), null, @"C:\Program Files"),
        new(1, "File1", 1234, new DateTime(2022, 12, 12), null, @"C:\Program Files"),
        new(1, "File1", 1234, new DateTime(2022, 12, 12), null, @"C:\Program Files"),
        new(1, "File1", 1234, new DateTime(2022, 12, 12), null, @"C:\Program Files"),
        new(1, "File1", 1234, new DateTime(2022, 12, 12), null, @"C:\Program Files"),
        new(1, "File1", 1234, new DateTime(2022, 12, 12), null, @"C:\Program Files"),
        new(1, "File1", 1234, new DateTime(2022, 12, 12), null, @"C:\Program Files"),
        new(1, "File1", 1234, new DateTime(2022, 12, 12), null, @"C:\Program Files"),
        new(1, "File1", 1234, new DateTime(2022, 12, 12), null, @"C:\Program Files"),
        new(2, "File2", 1234, new DateTime(1999, 1, 1), null, @"C:\Users\tczyz\Source\repos\LayersTraversing"),
        new(3, "File3", 144234, new DateTime(2022, 2, 12), null, @"C:\Program Files"),
        new(4, "FileFile4", 13234, new DateTime(202, 12, 30), null, @"C:\Users\tczyz\Source"),
        new(5, "File5", 122334, new DateTime(1990, 12, 30), null, @"C:\Users\tczyz\Source\repos\LayersTraversing\file.txt")
    };
}
