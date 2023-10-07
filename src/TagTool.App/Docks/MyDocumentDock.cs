using CommunityToolkit.Mvvm.Input;
using Dock.Model.Mvvm.Controls;
using Microsoft.Extensions.DependencyInjection;
using TagTool.App.Core.ViewModels;
using TagTool.App.ViewModels.UserControls;

namespace TagTool.App.Docks;

public partial class MyDocumentDock : DocumentDock
{
    private readonly IServiceProvider _serviceProvider;

    public MyDocumentDock(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        CanCreateDocument = true;
        CreateDocument = CreateNewDocumentCommand;
    }

    [RelayCommand]
    private void CreateNewDocument(string type)
    {
        if (!CanCreateDocument)
        {
            return;
        }

        var (documentType, tabName) = type switch
        {
            "Tags Library" => (typeof(TagsLibraryViewModel), "TagsLibrary"),
            "Search" => (typeof(TaggableItemsSearchViewModel), "Search"),
            "File Explorer" => (typeof(FileSystemViewModel), "FileExplorer"),
            "Items Tagger" => (typeof(ItemsTaggingTabViewModel), "ItemsTagger"),
            "New Search Bar" => (typeof(TaggableItemsSearchBarViewModel), "NewSearchBar"),
            "Tags Relations" => (typeof(TagsAssociationsViewModel), "TagsRelations"),
            "Tasks Manager" => (typeof(TasksManagerViewModel), "TasksManager"),
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };

        var document = (Document)_serviceProvider.GetRequiredService(documentType);
        document.Title = tabName;

        Factory?.AddDockable(this, document);
        Factory?.SetActiveDockable(document);
        Factory?.SetFocusedDockable(this, document);
    }
}
