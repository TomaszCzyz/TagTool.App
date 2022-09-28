using Avalonia;
using CommunityToolkit.Mvvm.Input;
using Dock.Model.Mvvm.Controls;
using TagTool.App.Extensions;

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

        var document = Application.Current?.CreateInstance<TabContentViewModel>()!;
        document.Title = "Untitled";

        Factory?.AddDockable(this, document);
        Factory?.SetActiveDockable(document);
        Factory?.SetFocusedDockable(this, document);
    }
}
