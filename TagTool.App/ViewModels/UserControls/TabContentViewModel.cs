using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using Dock.Model.Mvvm.Controls;
using DynamicData;
using TagTool.App.Core.Models;

namespace TagTool.App.ViewModels.UserControls;

public partial class TabContentViewModel : Document
{
    public ObservableCollection<SimpleFile> Files { get; set; } = new();

    public ObservableCollection<object> EnteredTags { get; set; } = new();

    public TabContentViewModel()
    {
        Files.AddRange(_exampleFiles);
        EnteredTags.AddRange(
            new Tag[] {
                new("Tag1"),
                new("Audio"),
                new("Dog"),
                new("Picture"),
                new("Colleague"),
                new("Tag6"),
                new("Tag7"),
                new("LastTag") });
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

    private readonly SimpleFile[] _exampleFiles =
    {
        new(1, "File1.txt", 1234, new DateTime(2022, 12, 12), new DateTime(2022, 12, 12), @"C:\Program Files"),
        new(1, "File2.txt", 12311111114, new DateTime(2022, 12, 12), new DateTime(2022, 12, 12), @"C:\Program Files"),
        new(1, "File3.txt", 1234, new DateTime(2022, 12, 12), new DateTime(2022, 12, 12), @"C:\Program Files"),
        new(1, "File4.txt", 1212312334, new DateTime(2022, 12, 12), new DateTime(2022, 12, 12), @"C:\Program Files"),
        new(1, "File5.txt", 1234, new DateTime(2022, 12, 12), new DateTime(2022, 12, 12), @"C:\Program Files"),
        new(1, "File6.txt", 1222234, new DateTime(2022, 12, 12), new DateTime(2022, 12, 12), @"C:\Program Files"),
        new(1, "File1.txt", 1234, new DateTime(2022, 12, 12), new DateTime(2022, 12, 12), @"C:\Program Files"),
        new(1, "File1.txt", 1234, new DateTime(2022, 12, 12), new DateTime(2022, 12, 12), @"C:\Program Files"),
        new(1, "File1.txt", 1234, new DateTime(2022, 12, 12), new DateTime(2022, 12, 12), @"C:\Program Files"),
        new(1, "File1.txt", 1212334, new DateTime(2022, 12, 12), new DateTime(2022, 12, 12), @"C:\Program Files"),
        new(1, "File1.txt", 1234, new DateTime(2022, 12, 12), new DateTime(2022, 12, 12), @"C:\Program Files"),
        new(1, "File1.txt", 1234, new DateTime(2022, 12, 12), new DateTime(2022, 12, 12), @"C:\Program Files"),
        new(2, "File2.txt", 1234, new DateTime(1999, 1, 1), new DateTime(1999, 1, 1), @"C:\Users\tczyz\Source\repos\LayersTraversing"),
        new(3, "File3.txt", 144234, new DateTime(2022, 2, 12), null, @"C:\Program Files"),
        new(4, "FileFile4", 13234, new DateTime(202, 12, 30), null, @"C:\Users\tczyz\Source"),
        new(5, "File5", 122334, new DateTime(1990, 12, 30), null, @"C:\Users\tczyz\Source\repos\LayersTraversing\file.txt")
    };
}
