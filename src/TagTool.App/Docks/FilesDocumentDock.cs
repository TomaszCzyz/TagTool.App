using CommunityToolkit.Mvvm.Input;
using Dock.Model.Mvvm.Controls;
using Microsoft.Extensions.DependencyInjection;
using TagTool.App.ViewModels.UserControls;

namespace TagTool.App.Docks;

public class FilesDocumentDock : DocumentDock
{
    private readonly IServiceProvider _serviceProvider;

    public FilesDocumentDock(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
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

        var document = _serviceProvider.GetRequiredService<TabContentViewModel>();
        document.Title = "Untitled";

        Factory?.AddDockable(this, document);
        Factory?.SetActiveDockable(document);
        Factory?.SetFocusedDockable(this, document);
    }
}
