using CommunityToolkit.Mvvm.Input;
using Dock.Model.Mvvm.Controls;

namespace TagTool.App.ViewModels.Docks;

public class FilesDocumentDock : DocumentDock
{
    public FilesDocumentDock()
    {
        CreateDocument = new RelayCommand(CreateNewDocument);
    }

    private void CreateNewDocument()
    {
        if (!CanCreateDocument)
        {
            return;
        }

        // var document = new FileViewModel
        // {
        //     Path = string.Empty,
        //     Title = "Untitled",
        //     Text = "",
        //     Encoding = Encoding.Default.WebName
        // };

        var document = new TabContentViewModel
        {
            Title = "Untitled"
        };

        Factory?.AddDockable(this, document);
        Factory?.SetActiveDockable(document);
        Factory?.SetFocusedDockable(this, document);
    }
}
