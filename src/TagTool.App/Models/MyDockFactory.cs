using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Model.Mvvm;
using Dock.Model.Mvvm.Controls;
using Microsoft.Extensions.DependencyInjection;
using TagTool.App.Models.Docks;
using TagTool.App.ViewModels.UserControls;

namespace TagTool.App.Models;

public class MyDockFactory : Factory
{
    private readonly IServiceProvider _serviceProvider;
    private IRootDock? _rootDock;
    private IDocumentDock? _documentDock;

    public MyDocumentDock LeftDock { get; private set; } = null!;
    public MyDocumentDock RightDock { get; private set; } = null!;
    public MyDocumentDock CentralDock { get; private set; } = null!;

    public MyDockFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public override IRootDock CreateLayout()
    {
        var myTags1 = _serviceProvider.GetRequiredService<MyTagsViewModel>();
        var taggableItemsSearchViewModel = _serviceProvider.GetRequiredService<TaggableItemsSearchViewModel>();
        var fileSystemViewModel = _serviceProvider.GetRequiredService<FileSystemViewModel>();
        var fileSystemSearchViewModel = _serviceProvider.GetRequiredService<FileSystemSearchViewModel>();

        myTags1.Title = "MyTags";
        taggableItemsSearchViewModel.Title = "TagSearch";
        fileSystemViewModel.Title = "FileSystem";
        fileSystemSearchViewModel.Title = "Search";

        LeftDock = _serviceProvider.GetRequiredService<MyDocumentDock>();
        LeftDock.Proportion = 0.25;
        LeftDock.IsCollapsable = true;
        LeftDock.VisibleDockables = CreateList<IDockable>(myTags1);
        LeftDock.CanCreateDocument = true;

        RightDock = _serviceProvider.GetRequiredService<MyDocumentDock>();
        RightDock.Proportion = 0.25;
        RightDock.IsCollapsable = false;
        RightDock.CanCreateDocument = true;

        CentralDock = _serviceProvider.GetRequiredService<MyDocumentDock>();
        CentralDock.Proportion = double.NaN;
        CentralDock.IsCollapsable = true;
        CentralDock.VisibleDockables = CreateList<IDockable>(taggableItemsSearchViewModel, fileSystemViewModel, fileSystemSearchViewModel);
        CentralDock.CanCreateDocument = true;

        var mainLayout = new ProportionalDock
        {
            Orientation = Orientation.Horizontal,
            VisibleDockables = CreateList<IDockable>
            (
                LeftDock,
                new ProportionalDockSplitter(),
                CentralDock
                // new ProportionalDockSplitter(),
                // RightDock
            )
        };

        var rootDock = new RootDock
        {
            Title = "Default",
            IsCollapsable = false,
            VisibleDockables = CreateList<IDockable>(mainLayout),
            ActiveDockable = mainLayout,
            DefaultDockable = mainLayout
        };

        _documentDock = CentralDock;
        _rootDock = rootDock;

        return rootDock;
    }

    public override void InitLayout(IDockable layout)
    {
        DockableLocator = new Dictionary<string, Func<IDockable?>> { ["Root"] = () => _rootDock, ["Files"] = () => _documentDock, };
        // HostWindowLocator = new Dictionary<string, Func<IHostWindow>> { [nameof(IDockWindow)] = () => new HostWindow() };

        base.InitLayout(layout);
    }
}
