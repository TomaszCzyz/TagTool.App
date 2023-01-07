using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Model.Mvvm;
using Dock.Model.Mvvm.Controls;
using TagTool.App.Models.Docks;
using TagTool.App.ViewModels.UserControls;

namespace TagTool.App.Models;

public class MyDockFactory : Factory
{
    private IRootDock? _rootDock;
    private IDocumentDock? _documentDock;

    public MyDocumentDock LeftDock { get; private set; } = null!;
    public MyDocumentDock RightDock { get; private set; } = null!;
    public MyDocumentDock CentralDock { get; private set; } = null!;

    public override IRootDock CreateLayout()
    {
        var myTags1 = new MyTagsViewModel { Title = "MyTags" };
        var myTags2 = new MyTagsViewModel { Title = "MyTags" };
        var taggedItemsSearchViewModel = new TaggedItemsSearchViewModel { Title = "Search" };
        var fileSystemViewModel = new FileSystemViewModel { Title = "FileSystem" };

        LeftDock = new MyDocumentDock
        {
            Proportion = 0.25,
            ActiveDockable = null,
            IsCollapsable = true,
            VisibleDockables = CreateList<IDockable>(myTags1),
            HiddenDockables = CreateList<IDockable>(),
            CanCreateDocument = true
        };

        RightDock = new MyDocumentDock
        {
            Proportion = 0.25,
            ActiveDockable = null,
            IsCollapsable = true,
            VisibleDockables = CreateList<IDockable>(myTags2),
            HiddenDockables = CreateList<IDockable>(),
            CanCreateDocument = true
        };

        CentralDock = new MyDocumentDock
        {
            Id = "SideDock",
            CanPin = true,
            IsCollapsable = false,
            Proportion = double.NaN,
            ActiveDockable = null,
            VisibleDockables = CreateList<IDockable>(taggedItemsSearchViewModel, fileSystemViewModel),
            CanCreateDocument = true
        };

        var mainLayout = new ProportionalDock
        {
            Orientation = Orientation.Horizontal,
            VisibleDockables = CreateList<IDockable>
            (
                LeftDock,
                new ProportionalDockSplitter(),
                CentralDock,
                new ProportionalDockSplitter(),
                RightDock
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
