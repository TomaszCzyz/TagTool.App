using CommunityToolkit.Mvvm.Input;
using Dock.Model.Mvvm.Controls;
using TagTool.App.ViewModels.UserControls;

namespace TagTool.App.Models.Docks;

public partial class MyDocumentDock : DocumentDock
{
    public MyDocumentDock()
    {
        CreateDocument = CreateNewDocumentCommand;
    }

    [RelayCommand]
    private void CreateNewDocument(string type)
    {
        if (!CanCreateDocument)
        {
            return;
        }

        Document document = type switch
        {
            "My Tags" => new MyTagsViewModel { Title = "MyTags" },
            "Search" => new TaggedItemsSearchViewModel { Title = "Search" },
            "File Explorer" => new FileSystemViewModel { Title = "FileExplorer" },
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };

        Factory?.AddDockable(this, document);
        Factory?.SetActiveDockable(document);
        Factory?.SetFocusedDockable(this, document);
    }
}
