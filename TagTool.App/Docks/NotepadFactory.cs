using Dock.Avalonia.Controls;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Model.Mvvm;
using Dock.Model.Mvvm.Controls;
using Microsoft.Extensions.DependencyInjection;
using TagTool.App.ViewModels.UserControls;

namespace TagTool.App.Docks;

public class NotepadFactory : Factory
{
    private readonly IServiceProvider _serviceProvider;
    private IRootDock? _rootDock;
    private IDocumentDock? _documentDock;

    public NotepadFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public override IDocumentDock CreateDocumentDock() => _serviceProvider.GetRequiredService<FilesDocumentDock>();

    public override IRootDock CreateLayout()
    {
        var untitledFileViewModel = new FileViewModel
        {
            Title = "fileViewModel"
        };

        var untitledTabContentViewModel = _serviceProvider.GetRequiredService<TabContentViewModel>();
        untitledTabContentViewModel.Title = "Untitled";
        var untitledTabContentViewModel2 = _serviceProvider.GetRequiredService<TabContentViewModel>();
        untitledTabContentViewModel2.Title = "Untitled";

        var documentDock = new FilesDocumentDock(_serviceProvider)
        {
            Id = "Files",
            Title = "Files",
            IsCollapsable = false,
            Proportion = double.NaN,
            ActiveDockable = untitledFileViewModel,
            VisibleDockables = CreateList<IDockable>
            (
                untitledFileViewModel,
                untitledTabContentViewModel
            ),
            CanCreateDocument = false
        };

        var documentDock2 = new FilesDocumentDock(_serviceProvider)
        {
            Id = "Files",
            Title = "Files",
            IsCollapsable = false,
            Proportion = double.NaN,
            ActiveDockable = untitledFileViewModel,
            VisibleDockables = CreateList<IDockable>(untitledTabContentViewModel2),
            CanCreateDocument = false
        };

        // var tools = new ProportionalDock
        // {
        //     Proportion = 0.2,
        //     Orientation = Orientation.Vertical,
        //     VisibleDockables = CreateList<IDockable>()
        // };

        var windowLayout = CreateRootDock();
        windowLayout.Title = "Default";
        var windowLayoutContent = new ProportionalDock
        {
            Orientation = Orientation.Horizontal,
            IsCollapsable = false,
            VisibleDockables = CreateList<IDockable>
            (
                documentDock,
                new ProportionalDockSplitter(),
                documentDock2
            )
        };
        windowLayout.IsCollapsable = false;
        windowLayout.VisibleDockables = CreateList<IDockable>(windowLayoutContent);
        windowLayout.ActiveDockable = windowLayoutContent;

        var rootDock = CreateRootDock();

        rootDock.IsCollapsable = false;
        rootDock.VisibleDockables = CreateList<IDockable>(windowLayout);
        rootDock.ActiveDockable = windowLayout;
        rootDock.DefaultDockable = windowLayout;

        _documentDock = documentDock;
        _rootDock = rootDock;

        return rootDock;
    }

    public override void InitLayout(IDockable layout)
    {
        ContextLocator = new Dictionary<string, Func<object>> { ["Find"] = () => layout, ["Replace"] = () => layout };

        DockableLocator = new Dictionary<string, Func<IDockable?>> { ["Root"] = () => _rootDock, ["Files"] = () => _documentDock };

        HostWindowLocator = new Dictionary<string, Func<IHostWindow>> { [nameof(IDockWindow)] = () => new HostWindow() };

        base.InitLayout(layout);
    }
}
